using FoodDeliverySystem.Common.ApiResponse.Interfaces;
using FoodDeliverySystem.Common.ApiResponse.Models;

namespace FoodDeliverySystem.Common.ApiResponse.Factories.Extensions;

public static class ResponseFactoryExtensions
{
    public static ApiResponseWithData<T> CreateFromStatusCode<T>(this IResponseFactory<T> factory, int statusCode, string message, IEnumerable<ErrorDetail>? errors = null)
    {
        return statusCode switch
        {
            400 => ResponseFactory<T>.BadRequest(message, errors),
            401 => ResponseFactory<T>.Unauthorized(message, errors),
            403 => ResponseFactory<T>.Forbidden(message), // Forbidden không dùng errors
            404 => ResponseFactory<T>.NotFound(message, errors),
            500 => ResponseFactory<T>.InternalServerError(message, errors),
            _ => ResponseFactory<T>.InternalServerError(message, errors)
        };
    }
}