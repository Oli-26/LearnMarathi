# Commute audio

Builds the guided listening tracks for the Commute page from `wwwroot/data/commute.json`.

```bash
python3 -m venv .venv && .venv/bin/pip install -r requirements.txt
.venv/bin/python build_audio.py            # only episodes whose audio is missing or stale
.venv/bin/python build_audio.py --force    # rebuild everything
```

Needs internet (Microsoft Edge neural voices). Output goes to `wwwroot/audio/commute/`:
`<id>.mp3` plus `<id>.json` with the start time of every segment, which the player uses
to highlight the current line. Clips are cached in `.cache/` so edits only re-voice changed lines.
