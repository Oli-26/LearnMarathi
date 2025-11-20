using System.Data.SqlClient;
using Dapper;
using LearnMarathi.Models;

namespace LearnMarathi.Data;

public class BasicWordRepository : IBasicWordRepository
{
    private readonly string _connectionString;

    public BasicWordRepository(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection")
                           ?? throw new ArgumentNullException("Connection string not found");
    }

    public async Task<IEnumerable<BasicWord>> GetAllWordsAsync()
    {
        using var connection = new SqlConnection(_connectionString);
        var query = "SELECT * FROM BasicWords ORDER BY Category, Id";
        return await connection.QueryAsync<BasicWord>(query);
    }

    public async Task<IEnumerable<BasicWord>> GetWordsByCategoryAsync(string category)
    {
        using var connection = new SqlConnection(_connectionString);
        var query = "SELECT * FROM BasicWords WHERE Category = @Category ORDER BY Id";
        return await connection.QueryAsync<BasicWord>(query, new { Category = category });
    }
}
