namespace LearnMarathi.Models;

public class BasicWord
{
    public int Id { get; set; }
    public string MarathiWord { get; set; } = string.Empty;
    public string EnglishTranslation { get; set; } = string.Empty;
    public string Pronunciation { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty; // Greetings, Numbers, Colors, etc.
}
