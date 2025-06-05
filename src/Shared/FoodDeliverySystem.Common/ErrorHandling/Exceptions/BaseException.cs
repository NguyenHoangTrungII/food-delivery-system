using FoodDeliverySystem.Common.Responses;

namespace FoodDeliverySystem.Common.Exceptions;

public abstract class BaseException : Exception
{
    public string ErrorCode { get; }
    public int StatusCode { get; }
    public IEnumerable<ErrorDetail>? ErrorDetails { get; }

    protected BaseException(string message, string errorCode, int statusCode, IEnumerable<ErrorDetail>? errorDetails = null, Exception? innerException = null)
        : base(message, innerException)
    {
        ErrorCode = errorCode;
        StatusCode = statusCode;
        ErrorDetails = errorDetails;
    }
}