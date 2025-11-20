namespace LearnMarathi.Models;

public class MarathiCharacter
{
    public int Id { get; set; }
    public string MarathiChar { get; set; } = string.Empty;
    public string EnglishTranslation { get; set; } = string.Empty;
    public string Pronunciation { get; set; } = string.Empty;
    public string CharacterType { get; set; } = string.Empty; // Vowel, Consonant, etc.
}
