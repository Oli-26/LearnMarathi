namespace LearnMarathi.Models;

public class SentenceDrill
{
    public int Id { get; set; }
    public string English { get; set; } = string.Empty;
    public string Pronunciation { get; set; } = string.Empty;
    /// <summary>Correct tokens in order; the drill shuffles them together with Distractors.</summary>
    public List<string> Tokens { get; set; } = new();
    public List<string> Distractors { get; set; } = new();
    public string Category { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;

    public string Marathi => string.Join(" ", Tokens);
}
