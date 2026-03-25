using System.Text.Json;
using CalorieCounter.Api.Models;

namespace CalorieCounter.Api.Services;

public sealed class FoodSearchService
{
    private const int DefaultResultLimit = 25;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IReadOnlyList<FoodDataRecord> _foods;

    public FoodSearchService(IWebHostEnvironment environment)
    {
        var dataPath = Path.Combine(environment.ContentRootPath, "Data", "food-data.json");

        if (!File.Exists(dataPath))
        {
            throw new FileNotFoundException(
                $"Food data file was not found at '{dataPath}'. Run the USDA transform script first.",
                dataPath);
        }

        using var stream = File.OpenRead(dataPath);
        _foods = JsonSerializer.Deserialize<List<FoodDataRecord>>(stream, JsonOptions)
            ?? throw new InvalidOperationException("Food data file did not contain any records.");
    }

    public FoodSearchResponse Search(string query)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();
        var matches = _foods
            .Where(food => food.SearchName.Contains(normalizedQuery, StringComparison.Ordinal))
            .ToList();

        var results = matches
            .Take(DefaultResultLimit)
            .Select(food => new FoodSearchItem(
                food.FoodCode,
                food.DisplayName,
                food.PortionDescription,
                food.Calories))
            .ToList();

        return new FoodSearchResponse(
            query.Trim(),
            matches.Count,
            results,
            matches.Count == 0 ? "No matching foods were found." : null);
    }
}
