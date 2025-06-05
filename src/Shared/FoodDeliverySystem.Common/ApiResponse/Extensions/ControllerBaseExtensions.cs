using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FoodDeliverySystem.Common.ApiResponse.Extensions;

public static class ControllerBaseExtensions
{
    public static IActionResult ToActionResult<T>(this ControllerBase controller, ApiResponseWithData<T> response, int statusCode)
    {
        return controller.StatusCode(statusCode, response);
    }

    public static IActionResult OkResponse<T>(this ControllerBase controller, T data, string message = "Request successful.")
    {
        return controller.ToActionResult(ResponseFactory<T>.Ok(data, message), StatusCodes.Status200OK);
    }

    public static IActionResult CreatedResponse<T>(this ControllerBase controller, T data, string message = "Resource created successfully.")
    {
        return controller.ToActionResult(ResponseFactory<T>.Created(data, message), StatusCodes.Status201Created);
    }

    public static IActionResult BadRequestResponse<T>(this ControllerBase controller, string message = "Bad request.", IEnumerable<ErrorDetail>? errors = null)
    {
        return controller.ToActionResult(ResponseFactory<T>.BadRequest(message, errors), StatusCodes.Status400BadRequest);
    }

    public static IActionResult UnauthorizedResponse<T>(this ControllerBase controller, string message = "Unauthorized access.", IEnumerable<ErrorDetail>? errors = null)
    {
        return controller.ToActionResult(ResponseFactory<T>.Unauthorized(message, errors), StatusCodes.Status401Unauthorized);
    }

    public static IActionResult ForbiddenResponse<T>(this ControllerBase controller, string message = "Access forbidden.")
    {
        return controller.ToActionResult(ResponseFactory<T>.Forbidden(message), StatusCodes.Status403Forbidden);
    }

    public static IActionResult NotFoundResponse<T>(this ControllerBase controller, string message = "Resource not found.", IEnumerable<ErrorDetail>? errors = null)
    {
        return controller.ToActionResult(ResponseFactory<T>.NotFound(message, errors), StatusCodes.Status404NotFound);
    }

    public static IActionResult InternalServerErrorResponse<T>(this ControllerBase controller, string message = "An unexpected error occurred.", IEnumerable<ErrorDetail>? errors = null)
    {
        return controller.ToActionResult(ResponseFactory<T>.InternalServerError(message, errors), StatusCodes.Status500InternalServerError);
    }

    public static IActionResult ValidationErrorResponse<T>(this ControllerBase controller, IEnumerable<ErrorDetail> errors, string message = "Validation failed.")
    {
        return controller.ToActionResult(ResponseFactory<T>.ValidationError(errors, message), StatusCodes.Status400BadRequest);
    }
}