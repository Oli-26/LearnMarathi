using LearnMarathi.Models;

namespace LearnMarathi.Data;

public record Matra(string Sign, string Roman, string Name);
public record Conjunct(string Glyph, string Parts, string Roman, string Example, string Meaning);
public record Syllable(string Id, int ConsonantId, string Text, string Roman);

public static class BarakhadiData
{
    public const string SyllableDeck = "barakhadi";
    public const string ConjunctDeck = "conjuncts";

    public static readonly Matra[] Matras =
    {
        new("",   "a",  "अ"),  new("ा", "aa", "आ"),
        new("ि",  "i",  "इ"),  new("ी", "ee", "ई"),
        new("ु",  "u",  "उ"),  new("ू", "oo", "ऊ"),
        new("े",  "e",  "ए"),  new("ै", "ai", "ऐ"),
        new("ो",  "o",  "ओ"),  new("ौ", "au", "औ"),
        new("ं",  "an", "अं"), new("ः", "ah", "अः"),
    };

    public static readonly Conjunct[] Conjuncts =
    {
        new("क्ष", "क् + ष", "ksha", "क्षमा",   "forgiveness"),
        new("ज्ञ", "ज् + ञ", "dnya", "ज्ञान",   "knowledge"),
        new("त्र", "त् + र", "tra",  "मित्र",   "friend"),
        new("श्र", "श् + र", "shra", "श्रम",    "labour"),
        new("प्र", "प् + र", "pra",  "प्रश्न",   "question"),
        new("क्र", "क् + र", "kra",  "क्रम",    "order, sequence"),
        new("ग्र", "ग् + र", "gra",  "ग्राम",   "village"),
        new("स्त", "स् + त", "sta",  "नमस्ते",  "greetings"),
        new("स्थ", "स् + थ", "stha", "स्थान",   "place"),
        new("द्य", "द् + य", "dya",  "विद्या",  "learning"),
        new("न्न", "न् + न", "nna",  "अन्न",    "food, grain"),
        new("ल्ल", "ल् + ल", "lla",  "दिल्ली",  "Delhi"),
        new("म्ह", "म् + ह", "mha",  "म्हणून",  "therefore"),
        new("न्ह", "न् + ह", "nha",  "कान्हा",  "Kanha (name)"),
        new("ट्ट", "ट् + ट", "tta",  "पट्टा",   "belt, strap"),
        new("च्च", "च् + च", "ccha", "उच्च",    "high"),
    };

    /// <summary>"ka" → "k", so the matra's own vowel can be appended.</summary>
    public static string RomanBase(MarathiCharacter c)
    {
        var r = c.EnglishTranslation;
        return r.EndsWith("a") ? r[..^1] : r;
    }

    // Id keys on the matra's roman, not its index, so reordering Matras keeps SRS history.
    public static IEnumerable<Syllable> Syllables(IEnumerable<MarathiCharacter> consonants) =>
        consonants.SelectMany(c => Matras.Select(m =>
            new Syllable($"{c.Id}:{m.Roman}", c.Id, c.MarathiChar + m.Sign, RomanBase(c) + m.Roman)));
}
