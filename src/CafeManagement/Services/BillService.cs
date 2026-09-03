using CafeManagement.Models;
using Microsoft.Data.SqlClient;

namespace CafeManagement.Services;

public sealed class BillService(IConfiguration configuration)
{
    private static readonly string[] AllowedPortions = ["Half", "Full", "NA"];
    private const int MinimumQuantity = 1;
    private const int MaximumQuantity = 10;

    private SqlConnection CreateConnection() => new(configuration.GetConnectionString("CafeDatabase")
        ?? throw new InvalidOperationException("CafeDatabase connection string is not configured."));

    public async Task<IReadOnlyList<BillMenuOption>> GetOptionsAsync(CancellationToken cancellationToken)
    {
        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT MenuItemId, ItemName, Portion, Price
            FROM dbo.MenuItem
            ORDER BY ItemName, Portion, MenuItemId;
            """;

        var options = new List<BillMenuOption>();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        while (await reader.ReadAsync(cancellationToken))
        {
            options.Add(new BillMenuOption(reader.GetInt32(0), reader.GetString(1), reader.GetString(2), reader.GetDecimal(3)));
        }

        return options;
    }

    public async Task<(CalculatedBillLine? Line, IDictionary<string, string[]> Errors, bool NotFound)> CalculateAsync(
        CalculateBillRequest request, CancellationToken cancellationToken)
    {
        var errors = new Dictionary<string, string[]>();
        if (request.MenuItemId <= 0) errors["menuItemId"] = ["Select a valid menu item."];
        if (request.Portion is null || !AllowedPortions.Contains(request.Portion)) errors["portion"] = ["Portion must be Half, Full, or NA."];
        if (request.Quantity is < MinimumQuantity or > MaximumQuantity) errors["quantity"] = [$"Quantity must be between {MinimumQuantity} and {MaximumQuantity}."];
        if (errors.Count > 0) return (null, errors, false);

        await using var connection = CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var command = connection.CreateCommand();
        command.CommandText = """
            SELECT ItemName, Portion, Price
            FROM dbo.MenuItem
            WHERE MenuItemId = @MenuItemId AND Portion = @Portion;
            """;
        command.Parameters.AddWithValue("@MenuItemId", request.MenuItemId);
        command.Parameters.AddWithValue("@Portion", request.Portion!);

        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        if (!await reader.ReadAsync(cancellationToken)) return (null, errors, true);

        var price = reader.GetDecimal(2);
        return (new CalculatedBillLine(
            request.MenuItemId,
            reader.GetString(0),
            reader.GetString(1),
            request.Quantity,
            price,
            price * request.Quantity), errors, false);
    }
}