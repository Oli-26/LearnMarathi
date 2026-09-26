using System.Text.Json;
using Microsoft.JSInterop;

namespace LearnMarathi.Services;

/// <summary>A word the learner typed in themselves; reviewed as deck "custom".</summary>
public class CustomWord
{
    public string Id { get; set; } = "";
    public string Marathi { get; set; } = "";
    public string English { get; set; } = "";
    public string Roman { get; set; } = "";
    public DateTime AddedUtc { get; set; }
}

/// <summary>
/// The learner's "hard words" list. Entries are keyed by Marathi text rather than deck + id,
/// so starring a word covers every deck it appears in (Words, Listening, Commute…).
/// </summary>
public interface IHardWordsService
{
    event Action? Changed;
    Task<HashSet<string>> GetAllAsync();
    Task<bool> IsHardAsync(string marathi);
    Task SetAsync(string marathi, bool hard);
    Task<List<CustomWord>> GetCustomAsync();
    Task AddCustomAsync(string marathi, string english, string roman);
    Task RemoveCustomAsync(string id);
    Task<HashSet<string>> GetDismissedAsync();
    Task DismissSuggestionAsync(string marathi);
}

public class HardWordsService : IHardWordsService
{
    public const string CustomDeck = "custom";

    private const string HardKey = "lm_hard";
    private const string CustomKey = "lm_custom_words";
    private const string DismissedKey = "lm_hard_dismissed";

    private readonly IJSRuntime _js;
    private HashSet<string>? _hard;
    private List<CustomWord>? _custom;
    private HashSet<string>? _dismissed;

    public event Action? Changed;

    public HardWordsService(IJSRuntime js) { _js = js; }

    public async Task<HashSet<string>> GetAllAsync() => _hard ??= await LoadAsync<HashSet<string>>(HardKey);

    public async Task<bool> IsHardAsync(string marathi) => (await GetAllAsync()).Contains(marathi);

    public async Task SetAsync(string marathi, bool hard)
    {
        var set = await GetAllAsync();
        if (!(hard ? set.Add(marathi) : set.Remove(marathi))) return;
        await SaveAsync(HardKey, set);
        Changed?.Invoke();
    }

    public async Task<List<CustomWord>> GetCustomAsync() => _custom ??= await LoadAsync<List<CustomWord>>(CustomKey);

    public async Task AddCustomAsync(string marathi, string english, string roman)
    {
        var list = await GetCustomAsync();
        marathi = marathi.Trim();
        if (marathi.Length == 0 || list.Any(w => w.Marathi == marathi)) return;
        list.Add(new CustomWord
        {
            Id = Guid.NewGuid().ToString("N")[..8],
            Marathi = marathi,
            English = english.Trim(),
            Roman = roman.Trim(),
            AddedUtc = DateTime.UtcNow
        });
        await SaveAsync(CustomKey, list);
        await SetAsync(marathi, true);
    }

    public async Task RemoveCustomAsync(string id)
    {
        var list = await GetCustomAsync();
        var word = list.FirstOrDefault(w => w.Id == id);
        if (word == null) return;
        list.Remove(word);
        await SaveAsync(CustomKey, list);
        await SetAsync(word.Marathi, false);
        Changed?.Invoke();
    }

    public async Task<HashSet<string>> GetDismissedAsync() => _dismissed ??= await LoadAsync<HashSet<string>>(DismissedKey);

    public async Task DismissSuggestionAsync(string marathi)
    {
        var set = await GetDismissedAsync();
        if (set.Add(marathi)) await SaveAsync(DismissedKey, set);
        Changed?.Invoke();
    }

    private async Task<T> LoadAsync<T>(string key) where T : new()
    {
        var raw = await _js.InvokeAsync<string?>("lmStorageGet", key);
        if (string.IsNullOrWhiteSpace(raw)) return new T();
        try { return JsonSerializer.Deserialize<T>(raw) ?? new T(); }
        catch (JsonException) { return new T(); }
    }

    private Task SaveAsync<T>(string key, T value) =>
        _js.InvokeVoidAsync("lmStorageSet", key, JsonSerializer.Serialize(value)).AsTask();
}
