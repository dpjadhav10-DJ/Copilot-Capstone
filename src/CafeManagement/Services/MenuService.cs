using CafeManagement.Models;
using Microsoft.Data.SqlClient;

namespace CafeManagement.Services;

public sealed class MenuService(IConfiguration configuration)
{
    private static readonly string[] AllowedPortions = ["Half", "Full"];

    private SqlConnection CreateConnection() => new(configuration.GetConnectionString("CafeDatabase")
        ?? throw new InvalidOperationException("CafeDatabase connection string is not configured."));

    public async Task<MenuPage> GetPageAsync(int page, int pageSize, CancellationToken cancellationToken)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);

        await using var countCommand = connection.CreateCommand();
        countCommand.CommandText = "SELECT COUNT(*) FROM dbo.MenuItem;";
        var totalCount = Convert.ToInt32(await countCommand.ExecuteScalarAsync(cancellationToken));

        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT MenuItemId, ItemName, Portion, Price, CreatedAt
            FROM dbo.MenuItem
            ORDER BY CreatedAt DESC, MenuItemId DESC
            OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY;
            """;
        command.Parameters.AddWithValue("@Offset", (page - 1) * pageSize);
        command.Parameters.AddWithValue("@PageSize", pageSize);

        var items = new List<MenuItem>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            items.Add(new MenuItem(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetDecimal(3), reader.GetDateTime(4)));
        }

        return new MenuPage(items, totalCount, page, pageSize);
    }

    public async Task<(MenuItem? Item, IDictionary<string, string[]> Errors)> CreateAsync(
        CreateMenuItemRequest request, CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0) return (null, errors);

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            INSERT INTO dbo.MenuItem (ItemName, Portion, Price)
            OUTPUT INSERTED.MenuItemId, INSERTED.ItemName, INSERTED.Portion, INSERTED.Price, INSERTED.CreatedAt
            VALUES (@ItemName, @Portion, @Price);
            """;
        command.Parameters.AddWithValue("@ItemName", request.ItemName!.Trim());
        command.Parameters.AddWithValue("@Portion", request.Portion!);
        command.Parameters.AddWithValue("@Price", request.Price!.Value);
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        await reader.ReadAsync(cancellationToken);
        return (new MenuItem(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetDecimal(3), reader.GetDateTime(4)), errors);
    }

    public async Task<int> RemoveAsync(IReadOnlyList<int>? ids, CancellationToken cancellationToken)
    {
        if (ids is null || ids.Count == 0) return 0;
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = (SqlTransaction)await connection.BeginTransactionAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.Transaction = transaction;
        var parameters = ids.Distinct().Select((id, index) =>
        {
            var name = $"@id{index}";
            command.Parameters.AddWithValue(name, id);
            return name;
        }).ToArray();
        command.CommandText = $"DELETE FROM dbo.MenuItem WHERE MenuItemId IN ({string.Join(",", parameters)});";
        var removed = await command.ExecuteNonQueryAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
        return removed;
    }

    private static Dictionary<string, string[]> Validate(CreateMenuItemRequest request)
    {
        var errors = new Dictionary<string, string[]>();
        if (string.IsNullOrWhiteSpace(request.ItemName)) errors["itemName"] = ["Item Name is required."];
        if (request.Price is null || request.Price < 0) errors["price"] = ["Price must be zero or greater."];
        if (request.Portion is null || !AllowedPortions.Contains(request.Portion)) errors["portion"] = ["Portion must be Half or Full."];
        return errors;
    }
}
