using FoodDeliverySystem.Common.ApiResponse.Extensions;
using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace RestaurantService.API.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected readonly IMediator Mediator;

    protected BaseApiController(IMediator mediator)
    {
        Mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    /// <summary>
    /// Converts an <see cref="ApiResponseWithData{T}"/> to an <see cref="IActionResult"/> with appropriate HTTP status code.
    /// </summary>
    /// <typeparam name="T">The type of data in the response.</typeparam>
    /// <param name="response">The API response containing data or errors.</param>
    /// <returns>An <see cref="IActionResult"/> representing the HTTP response.</returns>
    protected IActionResult ToActionResult<T>(ApiResponseWithData<T> response)
    {
        return this.ToActionResult(response, GetStatusCode(response));
    }

    /// <summary>
    /// Determines the appropriate HTTP status code based on the <see cref="ApiResponseWithData{T}"/> status and errors.
    /// </summary>
    /// <typeparam name="T">The type of data in the response.</typeparam>
    /// <param name="response">The API response to evaluate.</param>
    /// <returns>The corresponding HTTP status code.</returns>
    private static int GetStatusCode<T>(ApiResponseWithData<T> response)
    {
        if (response.Status == "success")
        {
            return StatusCodes.Status200OK; // Default for GET (e.g., FindNearby)
        }

        var errorCode = response.Errors.FirstOrDefault()?.Code;
        return errorCode switch
        {
            "NOT_FOUND" => StatusCodes.Status404NotFound,
            "CONFLICT" => StatusCodes.Status409Conflict,
            "VALIDATION_ERROR" => StatusCodes.Status400BadRequest,
            "UNAUTHORIZED" => StatusCodes.Status401Unauthorized,
            "INTERNAL_ERROR" => StatusCodes.Status500InternalServerError,
            _ => StatusCodes.Status400BadRequest
        };
    }
}