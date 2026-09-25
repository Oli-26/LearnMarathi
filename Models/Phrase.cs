namespace LearnMarathi.Models;

public class Phrase
{
    public int Id { get; set; }
    public string MarathiPhrase { get; set; } = string.Empty;
    public string EnglishTranslation { get; set; } = string.Empty;
    public string Pronunciation { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public int Frequency { get; set; } = 999;
}
