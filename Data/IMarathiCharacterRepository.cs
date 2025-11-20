using LearnMarathi.Models;

namespace LearnMarathi.Data;

public interface IMarathiCharacterRepository
{
    Task<IEnumerable<MarathiCharacter>> GetAllCharactersAsync();
    Task<IEnumerable<MarathiCharacter>> GetCharactersByTypeAsync(string type);
}
