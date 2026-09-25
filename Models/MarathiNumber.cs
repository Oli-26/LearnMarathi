namespace LearnMarathi.Models;

public class MarathiNumber
{
    public int Value { get; set; }
    public string Marathi { get; set; } = string.Empty;
    public string Pronunciation { get; set; } = string.Empty;
    public string Digits { get; set; } = string.Empty;
    /// <summary>False for the irregular 21–99 forms that still need a native-speaker check.</summary>
    public bool Verified { get; set; }
}
