using System.Net.Http.Json;
using LearnMarathi.Models;

namespace LearnMarathi.Data;

public class PhraseRepository : IPhraseRepository
{
    private readonly HttpClient _http;
    private List<Phrase>? _phrases;

    public PhraseRepository(HttpClient http) { _http = http; }

    public async Task<IEnumerable<Phrase>> GetAllAsync()
    {
        _phrases ??= await _http.GetFromJsonAsync<List<Phrase>>("data/phrases.json");
        return _phrases ?? new List<Phrase>();
    }
}
