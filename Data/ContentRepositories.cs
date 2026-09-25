using System.Net.Http.Json;
using LearnMarathi.Models;

namespace LearnMarathi.Data;

public interface INumberRepository { Task<IEnumerable<MarathiNumber>> GetAllAsync(); }
public interface IVerbRepository { Task<IEnumerable<Verb>> GetAllAsync(); }
public interface ISentenceRepository { Task<IEnumerable<SentenceDrill>> GetAllAsync(); }

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
