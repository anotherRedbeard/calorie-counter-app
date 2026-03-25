using System.Text.Json;
using CalorieCounter.Api.Models;

namespace CalorieCounter.Api.Services;

public sealed class FoodSearchService
{
    private const int DefaultResultLimit = 25;
    private const int MaxIndexedNGramLength = 3;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly IReadOnlyList<FoodDataRecord> _foods;
    private readonly Dictionary<string, List<int>> _nGramIndex = new(StringComparer.Ordinal);

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

        BuildIndex();
    }

    public FoodSearchResponse Search(string query)
    {
        var normalizedQuery = query.Trim().ToLowerInvariant();
        var matches = FindCandidateFoods(normalizedQuery)
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

    private IEnumerable<FoodDataRecord> FindCandidateFoods(string normalizedQuery)
    {
        if (normalizedQuery.Length == 0)
        {
            return [];
        }

        var nGramLength = Math.Min(MaxIndexedNGramLength, normalizedQuery.Length);
        var queryNGrams = GenerateNGrams(normalizedQuery, nGramLength).Distinct(StringComparer.Ordinal).ToList();
        if (queryNGrams.Count == 0)
        {
            return [];
        }

        List<int>? candidateIndices = null;
        foreach (var queryNGram in queryNGrams)
        {
            if (!_nGramIndex.TryGetValue(queryNGram, out var indexedFoodPositions))
            {
                return [];
            }

            candidateIndices = candidateIndices is null
                ? indexedFoodPositions
                : IntersectSorted(candidateIndices, indexedFoodPositions);

            if (candidateIndices.Count == 0)
            {
                return [];
            }
        }

        return candidateIndices!.Select(index => _foods[index]);
    }

    private void BuildIndex()
    {
        for (var index = 0; index < _foods.Count; index++)
        {
            var searchName = _foods[index].SearchName;
            var maxLength = Math.Min(MaxIndexedNGramLength, searchName.Length);

            for (var nGramLength = 1; nGramLength <= maxLength; nGramLength++)
            {
                foreach (var nGram in GenerateNGrams(searchName, nGramLength).Distinct(StringComparer.Ordinal))
                {
                    if (!_nGramIndex.TryGetValue(nGram, out var indices))
                    {
                        indices = [];
                        _nGramIndex[nGram] = indices;
                    }

                    indices.Add(index);
                }
            }
        }
    }

    private static IEnumerable<string> GenerateNGrams(string value, int nGramLength)
    {
        for (var i = 0; i <= value.Length - nGramLength; i++)
        {
            yield return value.Substring(i, nGramLength);
        }
    }

    private static List<int> IntersectSorted(IReadOnlyList<int> left, IReadOnlyList<int> right)
    {
        var intersection = new List<int>();
        var i = 0;
        var j = 0;

        while (i < left.Count && j < right.Count)
        {
            var leftValue = left[i];
            var rightValue = right[j];

            if (leftValue == rightValue)
            {
                intersection.Add(leftValue);
                i++;
                j++;
                continue;
            }

            if (leftValue < rightValue)
            {
                i++;
            }
            else
            {
                j++;
            }
        }

        return intersection;
    }
}
