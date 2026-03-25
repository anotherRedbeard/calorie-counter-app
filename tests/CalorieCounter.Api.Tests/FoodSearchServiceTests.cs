using System.Text.Json;
using CalorieCounter.Api.Models;
using CalorieCounter.Api.Services;
using Microsoft.AspNetCore.Hosting;
using NSubstitute;

namespace CalorieCounter.Api.Tests;

public sealed class FoodSearchServiceTests : IDisposable
{
    private readonly string _tempDir;
    private readonly FoodSearchService _service;

    public FoodSearchServiceTests()
    {
        _tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var dataDir = Path.Combine(_tempDir, "Data");
        Directory.CreateDirectory(dataDir);

        var foods = new List<FoodDataRecord>
        {
            new("1", "Apple juice", "apple juice", 1m, "cup", "1 cup", 114m),
            new("2", "Applesauce", "applesauce", 1m, "cup", "1 cup", 100m),
            new("3", "Chocolate milk", "chocolate milk", 1m, "cup", "1 cup", 208m),
            new("4", "Skim milk", "skim milk", 1m, "cup", "1 cup", 83m),
            new("5", "1% milk (low fat)", "1% milk (low fat)", 1m, "cup", "1 cup", 102m),
            new("6", "Chicken breast", "chicken breast", 1m, "piece", "1 piece", 130m),
            new("7", "Chicken thigh", "chicken thigh", 1m, "piece", "1 piece", 150m),
            new("8", "Rice", "rice", 1m, "cup", "1 cup", 205m),
        };

        File.WriteAllText(
            Path.Combine(dataDir, "food-data.json"),
            JsonSerializer.Serialize(foods));

        var env = Substitute.For<IWebHostEnvironment>();
        env.ContentRootPath.Returns(_tempDir);

        _service = new FoodSearchService(env);
    }

    public void Dispose()
    {
        if (Directory.Exists(_tempDir))
            Directory.Delete(_tempDir, recursive: true);
    }

    [Fact]
    public void Search_WithoutWildcard_PerformsSubstringMatch()
    {
        var result = _service.Search("milk");
        Assert.Equal(3, result.TotalMatches);
        Assert.All(result.Results, r =>
            Assert.Contains("milk", r.DisplayName, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void Search_PrefixWildcard_MatchesFoodsStartingWithTerm()
    {
        var result = _service.Search("apple*");
        Assert.Equal(2, result.TotalMatches);
        Assert.Contains(result.Results, r => r.DisplayName == "Apple juice");
        Assert.Contains(result.Results, r => r.DisplayName == "Applesauce");
    }

    [Fact]
    public void Search_SuffixWildcard_MatchesFoodsEndingWithTerm()
    {
        var result = _service.Search("*milk");
        Assert.Equal(2, result.TotalMatches);
        Assert.Contains(result.Results, r => r.DisplayName == "Chocolate milk");
        Assert.Contains(result.Results, r => r.DisplayName == "Skim milk");
    }

    [Fact]
    public void Search_InfixWildcard_MatchesFoodsWithPattern()
    {
        var result = _service.Search("chick*breast");
        Assert.Equal(1, result.TotalMatches);
        Assert.Equal("Chicken breast", result.Results[0].DisplayName);
    }

    [Fact]
    public void Search_DoubleWildcard_MatchesContains()
    {
        var result = _service.Search("*milk*");
        Assert.Equal(3, result.TotalMatches);
    }

    [Fact]
    public void Search_WildcardIsCaseInsensitive()
    {
        var result = _service.Search("APPLE*");
        Assert.Equal(2, result.TotalMatches);
    }

    [Fact]
    public void Search_WildcardNoMatches_ReturnsWarning()
    {
        var result = _service.Search("xyz*zzz");
        Assert.Equal(0, result.TotalMatches);
        Assert.Empty(result.Results);
        Assert.NotNull(result.Warning);
    }

    [Fact]
    public void Search_OnlyWildcard_MatchesEverything()
    {
        var result = _service.Search("*");
        Assert.Equal(8, result.TotalMatches);
    }

    [Fact]
    public void Search_WithoutWildcard_PreservesExistingBehavior()
    {
        var result = _service.Search("rice");
        Assert.Equal(1, result.TotalMatches);
        Assert.Equal("Rice", result.Results[0].DisplayName);
    }
}
