using FoodDeliverySystem.Common.Responses;
using Microsoft.Extensions.Localization;

namespace FoodDeliverySystem.Common.ErrorCodes;

public static class ErrorCodeMap
{
    private static readonly Dictionary<string, string> ErrorDefinitions = new()
    {
        { "VALIDATION_ERROR", "VALIDATION_ERROR" },
        { "UNKNOWN_ERROR", "UNKNOWN_ERROR" }
    };

    public static void RegisterErrorCode(string errorCode, string resourceKey)
    {
        ErrorDefinitions[errorCode] = resourceKey;
    }

    public static ErrorDetail CreateErrorDetail(IStringLocalizer localizer, string errorCode, string field)
    {
        var resourceKey = ErrorDefinitions.GetValueOrDefault(errorCode, "UNKNOWN_ERROR");
        var message = localizer[resourceKey].Value;
        return new ErrorDetail(errorCode, field, message);
    }

    public static string GetDefaultMessage(string errorCode)
    {
        return errorCode switch
        {
            "VALIDATION_ERROR" => "Validation failed.",
            "UNKNOWN_ERROR" => "Unknown error.",
            _ => "Unknown error."
        };
    }
}