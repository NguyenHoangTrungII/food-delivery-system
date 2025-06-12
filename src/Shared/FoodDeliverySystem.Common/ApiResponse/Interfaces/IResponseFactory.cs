using FoodDeliverySystem.Common.ApiResponse.Models;

namespace FoodDeliverySystem.Common.ApiResponse.Interfaces;

public interface IResponseFactory<T>
{
    ApiResponseWithData<T> Ok(T data, string message = "Request successful.");
    ApiResponseWithData<T> Created(T data, string message = "Resource created successfully.");
    ApiResponseWithData<T> BadRequest(string message = "Bad request.", IEnumerable<ErrorDetail>? errors = null);
    ApiResponseWithData<T> Unauthorized(string message = "Unauthorized access.", IEnumerable<ErrorDetail>? errors = null);
    ApiResponseWithData<T> Forbidden(string message = "Access forbidden.");
    ApiResponseWithData<T> NotFound(string message = "Resource not found.", IEnumerable<ErrorDetail>? errors = null);
    ApiResponseWithData<T> InternalServerError(string message = "An unexpected error occurred.", IEnumerable<ErrorDetail>? errors = null);
    ApiResponseWithData<T> ValidationError(IEnumerable<ErrorDetail> errors, string message = "Validation failed.");

}