using FoodDeliverySystem.Common.ApiResponse.Interfaces;
using FoodDeliverySystem.Common.ApiResponse.Models;
using Microsoft.AspNetCore.Http;

namespace FoodDeliverySystem.Common.ApiResponse.Factories;

public class ResponseFactory<T> : IResponseFactory<T>
{
    /// <summary>
    /// Creates a successful response with HTTP 200 OK.
    /// </summary>
    public static ApiResponseWithData<T> Ok(T data, string message = "Request successful.")
    {
        return new ApiResponseWithData<T>("success", message, data);
    }

    /// <summary>
    /// Creates a successful response with HTTP 201 Created.
    /// </summary>
    public static ApiResponseWithData<T> Created(T data, string message = "Resource created successfully.")
    {
        return new ApiResponseWithData<T>("success", message, data);
    }

    /// <summary>
    /// Creates an error response with HTTP 400 Bad Request.
    /// </summary>
    public static ApiResponseWithData<T> BadRequest(string message = "Bad request.", IEnumerable<ErrorDetail>? errors = null)
    {
        return new ApiResponseWithData<T>("error", message, default, errors);
    }

    /// <summary>
    /// Creates an error response with HTTP 401 Unauthorized.
    /// </summary>
    public static ApiResponseWithData<T> Unauthorized(string message = "Unauthorized access.", IEnumerable<ErrorDetail>? errors = null)
    {
        return new ApiResponseWithData<T>("error", message, default, errors);
    }

    /// <summary>
    /// Creates an error response with HTTP 403 Forbidden.
    /// </summary>
    public static ApiResponseWithData<T> Forbidden(string message = "Access forbidden.")
    {
        return new ApiResponseWithData<T>("error", message, default);
    }

    /// <summary>
    /// Creates an error response with HTTP 404 Not Found.
    /// </summary>
    public static ApiResponseWithData<T> NotFound(string message = "Resource not found.", IEnumerable<ErrorDetail>? errors = null)
    {
        return new ApiResponseWithData<T>("error", message, default, errors);
    }

    /// <summary>
    /// Creates an error response with HTTP 500 Internal Server Error.
    /// </summary>
    public static ApiResponseWithData<T> InternalServerError(string message = "An unexpected error occurred.", IEnumerable<ErrorDetail>? errors = null)
    {
        return new ApiResponseWithData<T>("error", message, default, errors);
    }

    /// <summary>
    /// Creates an error response with HTTP 400 Bad Request for validation errors.
    /// </summary>
    public static ApiResponseWithData<T> ValidationError(IEnumerable<ErrorDetail> errors, string message = "Validation failed.")
    {
        return new ApiResponseWithData<T>("error", message, default, errors);
    }

    // Non-static methods for IResponseFactory<T>
    ApiResponseWithData<T> IResponseFactory<T>.Ok(T data, string message) => Ok(data, message);
    ApiResponseWithData<T> IResponseFactory<T>.Created(T data, string message) => Created(data, message);
    ApiResponseWithData<T> IResponseFactory<T>.BadRequest(string message, IEnumerable<ErrorDetail>? errors) => BadRequest(message, errors);
    ApiResponseWithData<T> IResponseFactory<T>.Unauthorized(string message, IEnumerable<ErrorDetail>? errors) => Unauthorized(message, errors);
    ApiResponseWithData<T> IResponseFactory<T>.Forbidden(string message) => Forbidden(message);
    ApiResponseWithData<T> IResponseFactory<T>.NotFound(string message, IEnumerable<ErrorDetail>? errors) => NotFound(message, errors);
    ApiResponseWithData<T> IResponseFactory<T>.InternalServerError(string message, IEnumerable<ErrorDetail>? errors) => InternalServerError(message, errors);
    ApiResponseWithData<T> IResponseFactory<T>.ValidationError(IEnumerable<ErrorDetail> errors, string message) => ValidationError(errors, message);
}