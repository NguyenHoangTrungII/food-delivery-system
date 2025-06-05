namespace FoodDeliverySystem.Common.ApiResponse.Models;

public class SuccessDetail
{
    public string Code { get; set; }
    public string Message { get; set; }

    public SuccessDetail(string code, string message)
    {
        Code = code ?? throw new ArgumentNullException(nameof(code));
        Message = message ?? throw new ArgumentNullException(nameof(message));
    }
}