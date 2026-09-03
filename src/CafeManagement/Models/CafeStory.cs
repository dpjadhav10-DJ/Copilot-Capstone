namespace CafeManagement.Models;

public sealed record CafeStory(
    int CafeStoryId,
    string StoryText,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);
