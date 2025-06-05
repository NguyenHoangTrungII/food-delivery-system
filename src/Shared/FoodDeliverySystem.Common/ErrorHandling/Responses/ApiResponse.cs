namespace FoodDeliverySystem.Common.Responses;

public class ApiResponse<T>
{
    public bool Success { get; init; }
    public int StatusCode { get; init; }
    public string Message { get; init; }
    public T? Data { get; init; }
    public IEnumerable<ErrorDetail>? Errors { get; init; }
    public DateTime Timestamp { get; init; }

    private ApiResponse(
        bool success,
        int statusCode,
        string message,
        T? data = default,
        IEnumerable<ErrorDetail>? errors = null)
    {
        Success = success;
        StatusCode = statusCode;
        Message = message;
        Data = data;
        Errors = errors;
        Timestamp = DateTime.UtcNow;
    }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Request was successful.", int statusCode = 200)
    {
        return new ApiResponse<T>(true, statusCode, message, data);
    }

    public static ApiResponse<T> ErrorResponse(string message, int statusCode = 400, IEnumerable<ErrorDetail>? errors = null)
    {
        return new ApiResponse<T>(false, statusCode, message, default, errors);
    }
}