namespace FoodDeliverySystem.Common.Responses;

public class ErrorDetail
{
    public string ErrorCode { get; init; }
    public string Field { get; init; }
    public string Description { get; init; }

    public ErrorDetail(string errorCode, string field, string description)
    {
        ErrorCode = errorCode ?? throw new ArgumentNullException(nameof(errorCode));
        Field = field ?? string.Empty;
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }
}