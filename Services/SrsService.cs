using System.Text.Json;
using Microsoft.JSInterop;

namespace LearnMarathi.Services;

public class SrsCard
{
    public string Id { get; set; } = "";
    public int Reps { get; set; } = 0;
    public int Lapses { get; set; } = 0;
    public double Ease { get; set; } = 2.5;
    public double IntervalDays { get; set; } = 0;
    public DateTime DueUtc { get; set; } = DateTime.UtcNow;
    public DateTime LastSeenUtc { get; set; } = DateTime.MinValue;
    public int CorrectTotal { get; set; } = 0;
    public int WrongTotal { get; set; } = 0;
}

public interface ISrsService
{
    Task<SrsCard> GetAsync(string deck, string id);
    Task RecordAsync(string deck, string id, bool correct);
    Task<List<string>> GetDueIdsAsync(string deck, IEnumerable<string> candidateIds, int max);
    Task<Dictionary<string, SrsCard>> GetAllAsync(string deck);
}

public class SrsService : ISrsService
{
    private readonly IJSRuntime _js;
    private readonly Dictionary<string, Dictionary<string, SrsCard>> _cache = new();

    public SrsService(IJSRuntime js) { _js = js; }

    private static string StorageKey(string deck) => $"lm_srs_{deck}";

    private async Task<Dictionary<string, SrsCard>> LoadDeckAsync(string deck)
    {
        if (_cache.TryGetValue(deck, out var existing)) return existing;
        var raw = await _js.InvokeAsync<string?>("lmStorageGet", StorageKey(deck));
        Dictionary<string, SrsCard> map = new();
        if (!string.IsNullOrWhiteSpace(raw))
        {
            try
            {
                var parsed = JsonSerializer.Deserialize<Dictionary<string, SrsCard>>(raw);
                if (parsed != null) map = parsed;
            }
            catch { /* corrupt; reset */ }
        }
        _cache[deck] = map;
        return map;
    }

    private async Task SaveDeckAsync(string deck)
    {
        if (!_cache.TryGetValue(deck, out var map)) return;
        var raw = JsonSerializer.Serialize(map);
        await _js.InvokeVoidAsync("lmStorageSet", StorageKey(deck), raw);
    }

    public async Task<SrsCard> GetAsync(string deck, string id)
    {
        var map = await LoadDeckAsync(deck);
        if (!map.TryGetValue(id, out var card))
        {
            card = new SrsCard { Id = id };
            map[id] = card;
        }
        return card;
    }

    public async Task RecordAsync(string deck, string id, bool correct)
    {
        var map = await LoadDeckAsync(deck);
        if (!map.TryGetValue(id, out var card))
        {
            card = new SrsCard { Id = id };
            map[id] = card;
        }

        var now = DateTime.UtcNow;
        card.LastSeenUtc = now;
        card.Reps++;

        if (correct)
        {
            card.CorrectTotal++;
            card.IntervalDays = card.IntervalDays switch
            {
                0 => 1,
                1 => 3,
                _ => Math.Round(card.IntervalDays * card.Ease, 2)
            };
            card.Ease = Math.Min(3.0, card.Ease + 0.1);
        }
        else
        {
            card.WrongTotal++;
            card.Lapses++;
            card.IntervalDays = 0; // see again today
            card.Ease = Math.Max(1.3, card.Ease - 0.2);
        }

        card.DueUtc = now.AddDays(card.IntervalDays);
        await SaveDeckAsync(deck);
    }

    public async Task<List<string>> GetDueIdsAsync(string deck, IEnumerable<string> candidateIds, int max)
    {
        var map = await LoadDeckAsync(deck);
        var now = DateTime.UtcNow;
        var withScore = candidateIds.Select(id =>
        {
            map.TryGetValue(id, out var card);
            // Priority: never-seen > overdue > weak (low correct ratio) > newest
            double priority;
            if (card == null) priority = double.MaxValue / 2;
            else
            {
                var overdueDays = (now - card.DueUtc).TotalDays;
                var total = card.CorrectTotal + card.WrongTotal;
                var wrongRatio = total > 0 ? (double)card.WrongTotal / total : 0;
                priority = overdueDays + wrongRatio * 5;
            }
            return (id, priority);
        })
        .OrderByDescending(t => t.priority)
        .Take(max)
        .Select(t => t.id)
        .ToList();
        return withScore;
    }

    public async Task<Dictionary<string, SrsCard>> GetAllAsync(string deck)
    {
        return await LoadDeckAsync(deck);
    }
}
