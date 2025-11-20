using LearnMarathi.Models;

namespace LearnMarathi.Data;

public interface IBasicWordRepository
{
    Task<IEnumerable<BasicWord>> GetAllWordsAsync();
    Task<IEnumerable<BasicWord>> GetWordsByCategoryAsync(string category);
}
