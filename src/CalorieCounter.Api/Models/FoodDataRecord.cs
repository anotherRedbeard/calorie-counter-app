namespace CalorieCounter.Api.Models;

public sealed record FoodDataRecord(
    string FoodCode,
    string DisplayName,
    string SearchName,
    decimal PortionAmount,
    string PortionDisplayName,
    string PortionDescription,
    decimal Calories
);
