using FoodDeliverySystem.Common.ApiResponse.Models;

namespace FoodDeliverySystem.Common.ApiResponse.Interfaces;

public interface IApiResponse
{
    string Status { get; }
    string Message { get; }
    string Timestamp { get; }
    IReadOnlyList<ErrorDetail> Errors { get; }
}