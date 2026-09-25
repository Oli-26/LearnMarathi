namespace LearnMarathi.Models;

public class CommuteEpisode
{
    public string Id { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string MarathiTitle { get; set; } = string.Empty;
    public string Topic { get; set; } = string.Empty;
    public int Level { get; set; }
    public Dictionary<string, CommuteSpeaker> Speakers { get; set; } = new();
    public List<CommuteLine> Lines { get; set; } = new();
    public List<CommuteVocab> Vocab { get; set; } = new();
    public List<CommuteQuestion> Questions { get; set; } = new();

    public string AudioUrl => $"audio/commute/{Id}.mp3";
    public string TimingUrl => $"audio/commute/{Id}.json";
}

public class CommuteSpeaker
{
    public string Name { get; set; } = string.Empty;
    public string Voice { get; set; } = string.Empty;
}

public class CommuteLine
{
    public string Speaker { get; set; } = string.Empty;
    public string Marathi { get; set; } = string.Empty;
    public string Roman { get; set; } = string.Empty;
    public string English { get; set; } = string.Empty;
}

public class CommuteVocab
{
    public string Marathi { get; set; } = string.Empty;
    public string Roman { get; set; } = string.Empty;
    public string English { get; set; } = string.Empty;
}

public class CommuteQuestion
{
    public string Question { get; set; } = string.Empty;
    public List<string> Options { get; set; } = new();
    public int Answer { get; set; }
}

/// <summary>Written by tools/commute/build_audio.py next to each episode's mp3.</summary>
public class CommuteTiming
{
    public double Duration { get; set; }
    public List<CommuteSegment> Segments { get; set; } = new();
}

/// <summary>Phase is cue, full, slow, english or repeat; Line is -1 for spoken cues.</summary>
public class CommuteSegment
{
    public double T { get; set; }
    public string Phase { get; set; } = string.Empty;
    public int Line { get; set; }
}
