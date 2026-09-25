using System.Net.Http.Json;
using LearnMarathi.Models;

namespace LearnMarathi.Data;

public class MarathiCharacterRepository : IMarathiCharacterRepository
{
    private readonly HttpClient _http;
    private List<MarathiCharacter>? _characters;

    public MarathiCharacterRepository(HttpClient http)
    {
        _http = http;
    }

    private async Task EnsureLoadedAsync()
    {
        _characters ??= await _http.GetFromJsonAsync<List<MarathiCharacter>>("data/characters.json");
    }

    public async Task<IEnumerable<MarathiCharacter>> GetAllCharactersAsync()
    {
        await EnsureLoadedAsync();
        return _characters!.OrderBy(c => c.Id);
    }

    public async Task<IEnumerable<MarathiCharacter>> GetCharactersByTypeAsync(string type)
    {
        await EnsureLoadedAsync();
        return _characters!.Where(c => c.CharacterType == type).OrderBy(c => c.Id);
    }
}
