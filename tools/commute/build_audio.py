"""Render each Commute episode into one guided, hands-free mp3 plus a segment timing file.

Track shape: full dialogue -> every line (slow Marathi, English, Marathi again, pause to repeat)
-> full dialogue again. See README.md for usage.
"""
import argparse
import asyncio
import hashlib
import json
import subprocess
from pathlib import Path

import edge_tts
import imageio_ffmpeg

ROOT = Path(__file__).resolve().parents[2]
SOURCE = ROOT / "wwwroot/data/commute.json"
OUT = ROOT / "wwwroot/audio/commute"
CACHE = Path(__file__).resolve().parent / ".cache"

VOICES = {"male": "mr-IN-ManoharNeural", "female": "mr-IN-AarohiNeural"}
ENGLISH = "en-IN-NeerjaNeural"
SLOW = "-30%"

RATE = 24000  # edge-tts output; everything is decoded to 16-bit mono PCM at this rate
FFMPEG = imageio_ffmpeg.get_ffmpeg_exe()


async def clip_pcm(text: str, voice: str | tuple, rate: str = "+0%") -> bytes:
    voice, pitch = voice if isinstance(voice, tuple) else (voice, "+0Hz")
    key = hashlib.sha1(f"{voice}|{rate}|{pitch}|{text}".encode()).hexdigest()
    mp3 = CACHE / f"{key}.mp3"
    if mp3.exists() and mp3.stat().st_size == 0:
        mp3.unlink()  # left behind by an interrupted download
    for attempt in range(3):
        if mp3.exists():
            break
        try:
            await edge_tts.Communicate(text, voice, rate=rate, pitch=pitch).save(str(mp3))
        except edge_tts.exceptions.NoAudioReceived:
            mp3.unlink(missing_ok=True)
            if attempt == 2:
                raise RuntimeError(f"no audio for {voice} {rate} {pitch}: {text!r}")
            await asyncio.sleep(2)
    return subprocess.run(
        [FFMPEG, "-v", "error", "-i", str(mp3), "-ar", str(RATE), "-ac", "1", "-f", "s16le", "-"],
        check=True, capture_output=True).stdout


def silence(seconds: float) -> bytes:
    return b"\x00\x00" * int(RATE * seconds)


class Track:
    def __init__(self):
        self.pcm = bytearray()
        self.segments = []

    @property
    def now(self) -> float:
        return round(len(self.pcm) / 2 / RATE, 2)

    def add(self, audio: bytes, phase: str, line: int = -1, gap: float = 0.0):
        self.segments.append({"t": self.now, "phase": phase, "line": line})
        self.pcm += audio + silence(gap)


async def build(ep: dict):
    speakers = ep["speakers"]
    lines = ep["lines"]
    # There is one voice per gender, so a second same-gender speaker is pitched down to tell them apart.
    first = next(iter(speakers.values()))["voice"]
    pitch = {k: "-12Hz" if i > 0 and v["voice"] == first else "+0Hz" for i, (k, v) in enumerate(speakers.items())}
    voice_of = lambda line: (VOICES[speakers[line["speaker"]]["voice"]], pitch[line["speaker"]])
    t = Track()

    async def full_pass(label: str):
        t.add(await clip_pcm(label, ENGLISH), "cue", gap=0.8)
        for i, line in enumerate(lines):
            t.add(await clip_pcm(line["marathi"], voice_of(line)), "full", i, gap=0.6)

    await full_pass(f"{ep['title']}. First, just listen.")
    t.pcm += silence(1.0)

    t.add(await clip_pcm("Now, line by line. Repeat each line in the pause.", ENGLISH), "cue", gap=1.0)
    for i, line in enumerate(lines):
        marathi, voice = line["marathi"], voice_of(line)
        t.add(await clip_pcm(marathi, voice, SLOW), "slow", i, gap=0.7)
        t.add(await clip_pcm(line["english"], ENGLISH), "english", i, gap=0.7)
        repeat = await clip_pcm(marathi, voice)
        # Pause long enough to say the line back yourself.
        t.add(repeat, "repeat", i, gap=max(1.5, len(repeat) / 2 / RATE + 0.5))

    t.pcm += silence(1.0)
    await full_pass("Now once more, all the way through.")
    t.add(await clip_pcm("End of episode.", ENGLISH), "cue", gap=0.5)

    OUT.mkdir(parents=True, exist_ok=True)
    subprocess.run(
        [FFMPEG, "-v", "error", "-y", "-f", "s16le", "-ar", str(RATE), "-ac", "1", "-i", "-",
         "-b:a", "32k", str(OUT / f"{ep['id']}.mp3")],
        input=bytes(t.pcm), check=True)
    timing = {"source": source_hash(ep), "duration": t.now, "segments": t.segments}
    (OUT / f"{ep['id']}.json").write_text(json.dumps(timing, ensure_ascii=False))
    print(f"{ep['id']}: {t.now / 60:.1f} min")


def source_hash(ep: dict) -> str:
    return hashlib.sha1(json.dumps(ep, sort_keys=True, ensure_ascii=False).encode()).hexdigest()[:12]


def is_stale(ep: dict) -> bool:
    timing = OUT / f"{ep['id']}.json"
    if not timing.exists() or not (OUT / f"{ep['id']}.mp3").exists():
        return True
    return json.loads(timing.read_text()).get("source") != source_hash(ep)


async def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--force", action="store_true")
    parser.add_argument("ids", nargs="*", help="only these episode ids")
    args = parser.parse_args()

    CACHE.mkdir(exist_ok=True)
    episodes = json.loads(SOURCE.read_text())
    for ep in episodes:
        if args.ids and ep["id"] not in args.ids:
            continue
        if args.force or is_stale(ep):
            await build(ep)


if __name__ == "__main__":
    asyncio.run(main())
