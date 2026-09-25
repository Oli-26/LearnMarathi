using System.Globalization;
using System.Text;

namespace LearnMarathi.Services;

/// <summary>What recogniseSpeech in index.html resolves to.</summary>
public record SpeechAttempt(bool Ok, List<string> Matches, string? Message);

public static class SpeechMatch
{
    /// <summary>
    /// True if any recogniser guess is close enough to the target. Recognisers are shaky on short
    /// Marathi words, so this allows a stray word around it and roughly one slip per six letters.
    /// </summary>
    public static bool IsMatch(string target, IEnumerable<string> heard)
    {
        var want = Normalise(target);
        if (want.Length == 0) return false;
        var allowed = Math.Max(want.Length >= 4 ? 1 : 0, want.Length / 6);

        foreach (var guess in heard)
        {
            var got = Normalise(guess);
            if (got.Length == 0) continue;
            if (got == want || got.Contains(want)) return true;
            if (Distance(got, want) <= allowed) return true;
        }
        return false;
    }

    // Drops spaces, punctuation, nukta and joiners, which recognisers insert inconsistently.
    private static string Normalise(string s)
    {
        var sb = new StringBuilder(s.Length);
        foreach (var ch in s.Normalize(NormalizationForm.FormD))
        {
            if (ch == '़' || ch == '‌' || ch == '‍') continue;
            var cat = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (char.IsWhiteSpace(ch) || char.IsPunctuation(ch) || cat == UnicodeCategory.OtherPunctuation) continue;
            sb.Append(char.ToLowerInvariant(ch));
        }
        return sb.ToString();
    }

    private static int Distance(string a, string b)
    {
        var prev = new int[b.Length + 1];
        var cur = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++) prev[j] = j;
        for (var i = 1; i <= a.Length; i++)
        {
            cur[0] = i;
            for (var j = 1; j <= b.Length; j++)
                cur[j] = Math.Min(Math.Min(cur[j - 1], prev[j]) + 1, prev[j - 1] + (a[i - 1] == b[j - 1] ? 0 : 1));
            (prev, cur) = (cur, prev);
        }
        return prev[b.Length];
    }
}
