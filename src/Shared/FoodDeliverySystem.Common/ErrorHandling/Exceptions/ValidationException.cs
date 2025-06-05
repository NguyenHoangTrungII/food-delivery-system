using FoodDeliverySystem.Common.Responses;

namespace FoodDeliverySystem.Common.Exceptions;

public class ValidationException : BaseException
{
    public ValidationException(string message, IEnumerable<ErrorDetail> errorDetails)
        : base(message, "VALIDATION_ERROR", 400, errorDetails)
    {
    }

    public ValidationException(string message, ErrorDetail detail)
        : this(message, new[] { detail })
    {
    }
}