using Microsoft.AspNetCore.Http;

namespace FoodDeliverySystem.Common.Responses;

public static class ResponseFactory<T>
{
    public static ApiResponse<T> Ok(T data, string message = "Request successful.")
    {
        return ApiResponse<T>.SuccessResponse(data, message, StatusCodes.Status200OK);
    }

    public static ApiResponse<T> Created(T data, string message = "Resource created successfully.")
    {
        return ApiResponse<T>.SuccessResponse(data, message, StatusCodes.Status201Created);
    }

    public static ApiResponse<T> BadRequest(string message = "Bad request.", IEnumerable<ErrorDetail>? errors = null)
    {
        return ApiResponse<T>.ErrorResponse(message, StatusCodes.Status400BadRequest, errors);
    }

    public static ApiResponse<T> Unauthorized(string message = "Unauthorized access.")
    {
        return ApiResponse<T>.ErrorResponse(message, StatusCodes.Status401Unauthorized);
    }

    public static ApiResponse<T> Forbidden(string message = "Access forbidden.")
    {
        return ApiResponse<T>.ErrorResponse(message, StatusCodes.Status403Forbidden);
    }

    public static ApiResponse<T> NotFound(string message = "Resource not found.")
    {
        return ApiResponse<T>.ErrorResponse(message, StatusCodes.Status404NotFound);
    }

    public static ApiResponse<T> InternalServerError(string message = "An unexpected error occurred.")
    {
        return ApiResponse<T>.ErrorResponse(message, StatusCodes.Status500InternalServerError);
    }

    public static ApiResponse<T> ValidationError(IEnumerable<ErrorDetail> errors, string message = "Validation failed.")
    {
        return ApiResponse<T>.ErrorResponse(message, StatusCodes.Status400BadRequest, errors);
    }
}