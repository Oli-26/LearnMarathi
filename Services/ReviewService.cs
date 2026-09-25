using LearnMarathi.Data;
using Microsoft.JSInterop;

namespace LearnMarathi.Services;

/// <summary>One reviewable item, flattened out of whichever deck it came from.</summary>
/// <remarks>Group is the finest topic bucket (word category, else deck); used to keep neighbours varied.</remarks>
public record ReviewCard(string Deck, string DeckLabel, string Id, string Front, string Back, string Pronunciation, string Group)
{
    /// <summary>Never studied; Review shows it as an introduction rather than a question.</summary>
    public bool IsNew { get; init; }
}

public enum Mastery { New, Learning, Known }

public record CardProgress(ReviewCard Card, Mastery Mastery, int Correct, int Wrong, bool Due);

public interface IReviewService
{
    Task<List<CardProgress>> GetProgressAsync();
    Task<List<ReviewCard>> GetAllCardsAsync();
    Task<List<ReviewCard>> GetDueAsync(int max);
    Task<int> GetDueCountAsync();
    Task MarkIntroducedAsync();
}

public class ReviewService : IReviewService
{
    private readonly IMarathiCharacterRepository _chars;
    private readonly IBasicWordRepository _words;
    private readonly IPhraseRepository _phrases;
    private readonly INumberRepository _numbers;
    private readonly IVerbRepository _verbs;
    private readonly ISentenceRepository _sentences;
    private readonly ISrsService _srs;

    private readonly IJSRuntime _js;

    private List<ReviewCard>? _cards;
    private List<ReviewCard> _newOrder = new();

    private const int NewPerDay = 5;
    private const string IntroKey = "lm_new_intro";

    public ReviewService(
        IMarathiCharacterRepository chars,
        IBasicWordRepository words,
        IPhraseRepository phrases,
        INumberRepository numbers,
        IVerbRepository verbs,
        ISentenceRepository sentences,
        ISrsService srs,
        IJSRuntime js)
    {
        _chars = chars;
        _words = words;
        _phrases = phrases;
        _numbers = numbers;
        _verbs = verbs;
        _sentences = sentences;
        _srs = srs;
        _js = js;
    }

    public async Task<List<ReviewCard>> GetAllCardsAsync()
    {
        if (_cards != null) return _cards;

        var cards = new List<ReviewCard>();

        var chars = (await _chars.GetAllCharactersAsync()).ToList();
        cards.AddRange(chars
            .Select(c => new ReviewCard("characters", "Characters", c.Id.ToString(), c.MarathiChar, c.EnglishTranslation, c.Pronunciation, "characters")));

        cards.AddRange(BarakhadiData.Syllables(chars.Where(c => c.CharacterType == "Consonant"))
            .Select(x => new ReviewCard(BarakhadiData.SyllableDeck, "Barakhadi", x.Id, x.Text, x.Roman, x.Roman, $"barakhadi:{x.ConsonantId}")));
        cards.AddRange(BarakhadiData.Conjuncts
            .Select(j => new ReviewCard(BarakhadiData.ConjunctDeck, "Conjuncts", j.Glyph, j.Glyph, j.Roman, j.Parts, "conjuncts")));

        var words = (await _words.GetAllWordsAsync()).ToList();
        var byFrequency = words.OrderBy(w => w.Frequency).ThenBy(w => w.Id).ToList();
        cards.AddRange(words
            .Select(w => new ReviewCard("words", "Words", w.Id.ToString(), w.MarathiWord, w.EnglishTranslation, w.Pronunciation, $"words:{w.Category}")));
        cards.AddRange(words
            .Select(w => new ReviewCard("listen_words", "Listening", w.Id.ToString(), w.MarathiWord, w.EnglishTranslation, w.Pronunciation, $"words:{w.Category}")));

        cards.AddRange((await _phrases.GetAllAsync())
            .Select(p => new ReviewCard("listen_phrases", "Phrases", p.Id.ToString(), p.MarathiPhrase, p.EnglishTranslation, p.Pronunciation, "phrases")));

        cards.AddRange((await _numbers.GetAllAsync()).Where(n => n.Verified)
            .Select(n => new ReviewCard("numbers", "Numbers", n.Value.ToString(), n.Marathi, n.Value.ToString(), n.Pronunciation, "numbers")));

        cards.AddRange((await _verbs.GetAllAsync())
            .Select(v => new ReviewCard("verbs", "Verbs", v.Id.ToString(), v.Infinitive, v.EnglishTranslation, v.Pronunciation, $"verbs:{v.Category}")));

        cards.AddRange((await _sentences.GetAllAsync())
            .Select(x => new ReviewCard("sentences", "Sentences", x.Id.ToString(), x.Marathi, x.English, x.Pronunciation, "sentences")));

        // New material arrives most-common-word first, with a verb or phrase mixed in now and then.
        var newWords = byFrequency.Select(w => cards.First(c => c.Deck == "words" && c.Id == w.Id.ToString()));
        var newOthers = Mix(cards.Where(c => c.Deck == "verbs"), cards.Where(c => c.Deck == "listen_phrases"), every: 2);
        _newOrder = Mix(newWords, newOthers, every: 3).ToList();

        _cards = cards;
        return cards;
    }

    /// <summary>Cards seen at least once whose interval has elapsed. New cards are not "due".</summary>
    private async Task<List<ReviewCard>> DueCardsAsync()
    {
        var cards = await GetAllCardsAsync();
        var now = DateTime.UtcNow;
        var due = new List<ReviewCard>();

        foreach (var group in cards.GroupBy(c => c.Deck))
        {
            var state = await _srs.GetAllAsync(group.Key);
            due.AddRange(group.Where(c =>
                state.TryGetValue(c.Id, out var card) && card.Reps > 0 && card.DueUtc <= now));
        }

        return due;
    }

    public async Task<List<ReviewCard>> GetDueAsync(int max)
    {
        var fresh = await NewCardsAsync(await NewAllowanceAsync());
        var due = await DueCardsAsync();
        // The same word can be due from both the Words and Listening decks; ask it once.
        var picked = due.OrderBy(_ => Random.Shared.Next()).DistinctBy(c => c.Front).Take(max - fresh.Count).ToList();
        return SpreadByGroup(picked.Concat(fresh).ToList());
    }

    /// <summary>Next unstudied cards in teaching order. A word met in any deck (e.g. Listening) is not new.</summary>
    private async Task<List<ReviewCard>> NewCardsAsync(int count)
    {
        if (count <= 0) return new();
        var cards = await GetAllCardsAsync();
        var seenFronts = new HashSet<string>();
        foreach (var group in cards.GroupBy(c => c.Deck))
        {
            var state = await _srs.GetAllAsync(group.Key);
            seenFronts.UnionWith(group.Where(c => state.TryGetValue(c.Id, out var s) && s.Reps > 0).Select(c => c.Front));
        }
        return _newOrder.Where(c => !seenFronts.Contains(c.Front)).Take(count).Select(c => c with { IsNew = true }).ToList();
    }

    private async Task<int> NewAllowanceAsync()
    {
        var (day, used) = await ReadIntroCounterAsync();
        return day == DateTime.Now.Date ? Math.Max(0, NewPerDay - used) : NewPerDay;
    }

    public async Task MarkIntroducedAsync()
    {
        var (day, used) = await ReadIntroCounterAsync();
        var today = DateTime.Now.Date;
        var count = day == today ? used + 1 : 1;
        await _js.InvokeVoidAsync("lmStorageSet", IntroKey, $"{today:yyyy-MM-dd}|{count}");
    }

    private async Task<(DateTime? Day, int Used)> ReadIntroCounterAsync()
    {
        var raw = await _js.InvokeAsync<string?>("lmStorageGet", IntroKey);
        var parts = raw?.Split('|');
        if (parts?.Length == 2 && DateTime.TryParse(parts[0], out var day) && int.TryParse(parts[1], out var used))
            return (day.Date, used);
        return (null, 0);
    }

    /// <summary>Yields from primary, slotting in one item from secondary at every Nth position.</summary>
    private static IEnumerable<T> Mix<T>(IEnumerable<T> primary, IEnumerable<T> secondary, int every)
    {
        using var a = primary.GetEnumerator();
        using var b = secondary.GetEnumerator();
        bool moreA = true, moreB = true;
        for (var i = 1; moreA || moreB; i++)
        {
            if (i % every == 0 && moreB && (moreB = b.MoveNext())) { yield return b.Current; continue; }
            if (moreA && (moreA = a.MoveNext())) { yield return a.Current; continue; }
            if (moreB && (moreB = b.MoveNext())) yield return b.Current;
        }
    }

    /// <summary>Reorders so consecutive cards come from different groups wherever possible.</summary>
    private static List<ReviewCard> SpreadByGroup(List<ReviewCard> cards)
    {
        var remaining = new List<ReviewCard>(cards);
        var result = new List<ReviewCard>(cards.Count);
        while (remaining.Count > 0)
        {
            var last = result.Count > 0 ? result[^1].Group : null;
            // Draw from the largest other group so a dominant category can't pile up at the end.
            var next = remaining
                .Where(c => c.Group != last)
                .GroupBy(c => c.Group)
                .OrderByDescending(g => g.Count())
                .ThenBy(_ => Random.Shared.Next())
                .Select(g => g.First())
                .FirstOrDefault() ?? remaining[0];
            remaining.Remove(next);
            result.Add(next);
        }
        return result;
    }

    // Three straight correct answers reach this interval (1 → 3 → 7.5 days).
    private const double KnownIntervalDays = 7;

    public async Task<List<CardProgress>> GetProgressAsync()
    {
        var cards = await GetAllCardsAsync();
        var now = DateTime.UtcNow;
        var result = new List<CardProgress>(cards.Count);

        foreach (var group in cards.GroupBy(c => c.Deck))
        {
            var state = await _srs.GetAllAsync(group.Key);
            foreach (var c in group)
            {
                if (!state.TryGetValue(c.Id, out var s) || s.Reps == 0)
                {
                    result.Add(new CardProgress(c, Mastery.New, 0, 0, false));
                    continue;
                }
                var mastery = s.IntervalDays >= KnownIntervalDays ? Mastery.Known : Mastery.Learning;
                result.Add(new CardProgress(c, mastery, s.CorrectTotal, s.WrongTotal, s.DueUtc <= now));
            }
        }

        return result;
    }

    public async Task<int> GetDueCountAsync() =>
        (await DueCardsAsync()).DistinctBy(c => c.Front).Count() + (await NewCardsAsync(await NewAllowanceAsync())).Count;
}
