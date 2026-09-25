using LearnMarathi.Models;

namespace LearnMarathi.Data;

public interface IPhraseRepository
{
    Task<IEnumerable<Phrase>> GetAllAsync();
}
