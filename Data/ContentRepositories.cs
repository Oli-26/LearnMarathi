using System.Net.Http.Json;
using LearnMarathi.Models;

namespace LearnMarathi.Data;

public interface INumberRepository { Task<IEnumerable<MarathiNumber>> GetAllAsync(); }
public interface IVerbRepository { Task<IEnumerable<Verb>> GetAllAsync(); }
public interface ISentenceRepository { Task<IEnumerable<SentenceDrill>> GetAllAsync(); }
public interface ICommuteRepository
{
    Task<IEnumerable<CommuteEpisode>> GetAllAsync();
    Task<CommuteTiming?> GetTimingAsync(CommuteEpisode episode);
}

public class NumberRepository : INumberRepository
{
    private readonly HttpClient _http;
    private List<MarathiNumber>? _cache;

    public NumberRepository(HttpClient http) { _http = http; }

    public async Task<IEnumerable<MarathiNumber>> GetAllAsync()
    {
        _cache ??= await _http.GetFromJsonAsync<List<MarathiNumber>>("data/numbers.json");
        return _cache ?? new List<MarathiNumber>();
    }
}

public class VerbRepository : IVerbRepository
{
    private readonly HttpClient _http;
    private List<Verb>? _cache;

    public VerbRepository(HttpClient http) { _http = http; }

    public async Task<IEnumerable<Verb>> GetAllAsync()
    {
        _cache ??= await _http.GetFromJsonAsync<List<Verb>>("data/verbs.json");
        return _cache ?? new List<Verb>();
    }
}

public class SentenceRepository : ISentenceRepository
{
    private readonly HttpClient _http;
    private List<SentenceDrill>? _cache;

    public SentenceRepository(HttpClient http) { _http = http; }

    public async Task<IEnumerable<SentenceDrill>> GetAllAsync()
    {
        _cache ??= await _http.GetFromJsonAsync<List<SentenceDrill>>("data/sentences.json");
        return _cache ?? new List<SentenceDrill>();
    }
}

public class CommuteRepository : ICommuteRepository
{
    private readonly HttpClient _http;
    private List<CommuteEpisode>? _cache;
    private readonly Dictionary<string, CommuteTiming?> _timings = new();

    public CommuteRepository(HttpClient http) { _http = http; }

    public async Task<IEnumerable<CommuteEpisode>> GetAllAsync()
    {
        _cache ??= await _http.GetFromJsonAsync<List<CommuteEpisode>>("data/commute.json");
        return _cache ?? new List<CommuteEpisode>();
    }

    public async Task<CommuteTiming?> GetTimingAsync(CommuteEpisode episode)
    {
        if (_timings.TryGetValue(episode.Id, out var cached)) return cached;
        CommuteTiming? timing = null;
        try { timing = await _http.GetFromJsonAsync<CommuteTiming>(episode.TimingUrl); }
        catch (HttpRequestException) { /* audio not built yet */ }
        _timings[episode.Id] = timing;
        return timing;
    }
}
