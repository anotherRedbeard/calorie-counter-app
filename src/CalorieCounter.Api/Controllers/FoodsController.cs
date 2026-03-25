using CalorieCounter.Api.Models;
using CalorieCounter.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace CalorieCounter.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class FoodsController : ControllerBase
{
    private readonly FoodSearchService _foodSearchService;

    public FoodsController(FoodSearchService foodSearchService)
    {
        _foodSearchService = foodSearchService;
    }

    [HttpGet("search")]
    [ProducesResponseType<FoodSearchResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<FoodSearchResponse> Search([FromQuery] string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Search term is required.",
                Detail = "Enter a food description before searching.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        return Ok(_foodSearchService.Search(q));
    }
}
