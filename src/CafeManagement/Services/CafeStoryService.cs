using CafeManagement.Models;
using Microsoft.Data.SqlClient;

namespace CafeManagement.Services;

public sealed class CafeStoryService(IConfiguration configuration)
{
    public async Task<CafeStory?> GetActiveStoryAsync(CancellationToken cancellationToken)
    {
        var connectionString = configuration.GetConnectionString("CafeDatabase")
            ?? throw new InvalidOperationException("CafeDatabase connection string is not configured.");

        await using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT TOP (1) CafeStoryId, StoryText, IsActive, CreatedAt, UpdatedAt
            FROM dbo.CafeStory
            WHERE IsActive = 1
            ORDER BY UpdatedAt DESC, CafeStoryId DESC;
            """;

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken))
        {
            return null;
        }

        return new CafeStory(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetBoolean(2),
            reader.GetDateTime(3),
            reader.GetDateTime(4));
    }
}
