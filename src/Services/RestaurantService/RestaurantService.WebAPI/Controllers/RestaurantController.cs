using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Geo.Models;
using FoodDeliverySystem.Common.Logging;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using RestaurantService.Application.Dtos;
using RestaurantService.Application.Queries;
using System.ComponentModel.DataAnnotations;

namespace RestaurantService.API.Controllers;

[Route("api/restaurant")]
[ApiController]
public class RestaurantController : BaseApiController
{
    private readonly ILoggerAdapter<RestaurantController> _logger;

    public RestaurantController(IMediator mediator, ILoggerAdapter<RestaurantController> logger) : base(mediator)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Finds restaurants within a specified radius from a given location.
    /// </summary>
    /// <param name="latitude">The latitude of the center point (between -90 and 90).</param>
    /// <param name="longitude">The longitude of the center point (between -180 and 180).</param>
    /// <param name="radius">The radius in the specified unit (default: 5 kilometers).</param>
    /// <param name="unit">The unit of measurement for the radius (default: Kilometers).</param>
    /// <returns>An <see cref="ApiResponseWithData{IEnumerable{RestaurantDto}}"/> containing the list of nearby restaurants.</returns>
    /// <response code="200">Restaurants found successfully.</response>
    /// <response code="400">Invalid query parameters (e.g., invalid latitude/longitude).</response>
    /// <response code="500">Server error occurred.</response>
    [HttpGet("nearby")]
    [ProducesResponseType(typeof(ApiResponseWithData<IEnumerable<RestaurantDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponseWithData<IEnumerable<RestaurantDto>>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponseWithData<IEnumerable<RestaurantDto>>), StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> FindNearby(
        [FromQuery][Required] double latitude,
        [FromQuery][Required] double longitude,
        [FromQuery][Range(0.1, 100)] double radius = 5,
        [FromQuery] GeoUnit unit = GeoUnit.Kilometers)
    {
        _logger.LogInformation("Finding restaurants near ({Latitude}, {Longitude}) within {Radius} {Unit}",
            latitude, longitude, radius, unit);

        var query = new FindRestaurantsNearbyQuery(latitude, longitude, radius, unit);
        var response = await Mediator.Send(query);
        return ToActionResult(response);
    }
}