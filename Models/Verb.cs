namespace LearnMarathi.Models;

public class Verb
{
    public int Id { get; set; }
    public string Infinitive { get; set; } = string.Empty;
    /// <summary>Stem the present-habitual endings attach to (वाच → वाचतो; पि → पितो).</summary>
    public string PresentStem { get; set; } = string.Empty;
    public string Pronunciation { get; set; } = string.Empty;
    public string EnglishTranslation { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}
