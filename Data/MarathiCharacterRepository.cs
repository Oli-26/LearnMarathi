using System.Data.SqlClient;
using Dapper;
using LearnMarathi.Models;

namespace LearnMarathi.Data;

public class MarathiCharacterRepository : IMarathiCharacterRepository
{
    private readonly string _connectionString;

    public MarathiCharacterRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                            ?? throw new ArgumentNullException("Connection string not found");
    }

    public async Task<IEnumerable<MarathiCharacter>> GetAllCharactersAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        connection.Open();
        var query = "SELECT [Id],[MarathiChar] ,[EnglishTranslation] ,[Pronunciation], [CharacterType] FROM MarathiCharacters ORDER BY Id";
        return await connection.QueryAsync<MarathiCharacter>(query);
    }
 
    public async Task<IEnumerable<MarathiCharacter>> GetCharactersByTypeAsync(string type)
    {
        using var connection = new SqlConnection(_connectionString);
        var query = "SELECT * FROM MarathiCharacters WHERE CharacterType = @Type ORDER BY Id";
        return await connection.QueryAsync<MarathiCharacter>(query, new { Type = type });
    }
}