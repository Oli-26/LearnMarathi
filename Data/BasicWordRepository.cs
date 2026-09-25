using System.Net.Http.Json;
using LearnMarathi.Models;

namespace LearnMarathi.Data;

public class BasicWordRepository : IBasicWordRepository
{
    private readonly HttpClient _http;
    private List<BasicWord>? _words;

    public BasicWordRepository(HttpClient http)
    {
        _http = http;
    }

    private async Task EnsureLoadedAsync()
    {
        _words ??= await _http.GetFromJsonAsync<List<BasicWord>>("data/words.json");
    }

    public async Task<IEnumerable<BasicWord>> GetAllWordsAsync()
    {
        await EnsureLoadedAsync();
        return _words!.OrderBy(w => w.Category).ThenBy(w => w.Id);
    }

    public async Task<IEnumerable<BasicWord>> GetWordsByCategoryAsync(string category)
    {
        await EnsureLoadedAsync();
        return _words!.Where(w => w.Category == category).OrderBy(w => w.Id);
    }
}
