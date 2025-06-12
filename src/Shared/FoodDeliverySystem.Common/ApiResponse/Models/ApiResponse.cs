using FoodDeliverySystem.Common.ApiResponse.Interfaces;

namespace FoodDeliverySystem.Common.ApiResponse.Models;

public abstract class ApiResponse:  IApiResponse
{
    public string Status { get; set; }
    public string Message { get; set; }
    public string Timestamp { get; set; }
    public IReadOnlyList<ErrorDetail> Errors { get; set; }
    public ApiResponse() {
        Status = string.Empty;
        Message = string.Empty;
        Timestamp = DateTime.UtcNow.ToString("O");
        Errors = Array.Empty<ErrorDetail>().AsReadOnly();
    }


    protected ApiResponse(string status, string message, IEnumerable<ErrorDetail>? errors = null)
    {
        Status = status ?? throw new ArgumentNullException(nameof(status));
        Message = message ?? string.Empty;
        Timestamp = DateTime.UtcNow.ToString("O");
        Errors = errors?.ToList().AsReadOnly() ?? Array.Empty<ErrorDetail>().AsReadOnly();
    }
}