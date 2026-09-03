using CafeManagement.Services;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:8080");
builder.Services.AddSingleton<CafeStoryService>();

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

app.MapFallbackToFile("index.html");
app.Run();

public partial class Program;
