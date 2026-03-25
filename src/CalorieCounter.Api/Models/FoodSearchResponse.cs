namespace CalorieCounter.Api.Models;

public sealed record FoodSearchItem(
    string FoodCode,
    string DisplayName,
    string PortionDescription,
    decimal Calories
);

public sealed record FoodSearchResponse(
    string Query,
    int TotalMatches,
    IReadOnlyList<FoodSearchItem> Results,
    string? Warning
);
