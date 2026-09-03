namespace CafeManagement.Models;

public sealed record MenuItem(
    int MenuItemId,
    string ItemName,
    string Portion,
    decimal Price,
    DateTime CreatedAt);

public sealed record MenuPage(
    IReadOnlyList<MenuItem> Items,
    int TotalCount,
    int Page,
    int PageSize);

public sealed record CreateMenuItemRequest(
    string? ItemName,
    string? Portion,
    decimal? Price);

public sealed record RemoveMenuItemsRequest(IReadOnlyList<int>? MenuItemIds);
