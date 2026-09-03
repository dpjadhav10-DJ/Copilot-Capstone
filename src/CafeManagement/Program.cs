using CafeManagement.Models;
using CafeManagement.Services;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:8080");
builder.Services.AddSingleton<CafeStoryService>();
builder.Services.AddSingleton<MenuService>();
builder.Services.AddSingleton<BillService>();

var app = builder.Build();
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapGet("/api/cafe-story/active", async (CafeStoryService storyService, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    try
    {
        var story = await storyService.GetActiveStoryAsync(cancellationToken);
        return story is null
            ? Results.NotFound(new { message = "The cafe story is currently unavailable." })
            : Results.Ok(story);
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Cafe story retrieval failed.");
        return Results.Problem("The cafe story is currently unavailable.", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapGet("/api/menu", async (int? page, int? pageSize, MenuService menuService, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await menuService.GetPageAsync(page ?? 1, pageSize ?? 10, cancellationToken));
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Menu retrieval failed.");
        return Results.Problem("The cafe menu is currently unavailable.", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapPost("/api/menu", async ([FromBody] CreateMenuItemRequest request, [FromServices] MenuService menuService, [FromServices] ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    try
    {
        var result = await menuService.CreateAsync(request, cancellationToken);
        return result.Errors.Count > 0 ? Results.ValidationProblem(result.Errors) : Results.Ok(result.Item);
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Menu creation failed.");
        return Results.Problem("The menu item could not be saved.", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapDelete("/api/menu", async ([FromBody] RemoveMenuItemsRequest request, [FromServices] MenuService menuService, [FromServices] ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(new { removed = await menuService.RemoveAsync(request.MenuItemIds, cancellationToken) });
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Menu removal failed.");
        return Results.Problem("The menu item could not be removed.", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapGet("/api/menu/bill-options", async (BillService billService, ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    try
    {
        return Results.Ok(await billService.GetOptionsAsync(cancellationToken));
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Bill menu options retrieval failed.");
        return Results.Problem("The bill menu is currently unavailable.", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapPost("/api/bill/calculate", async ([FromBody] CalculateBillRequest request, [FromServices] BillService billService, [FromServices] ILogger<Program> logger, CancellationToken cancellationToken) =>
{
    try
    {
        var result = await billService.CalculateAsync(request, cancellationToken);
        if (result.Errors.Count > 0) return Results.ValidationProblem(result.Errors);
        return result.NotFound
            ? Results.NotFound(new { message = "The selected menu item and portion are unavailable." })
            : Results.Ok(result.Line);
    }
    catch (Exception exception)
    {
        logger.LogError(exception, "Bill calculation failed.");
        return Results.Problem("The bill amount could not be calculated.", statusCode: StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapFallbackToFile("index.html");
app.Run();

public partial class Program;
