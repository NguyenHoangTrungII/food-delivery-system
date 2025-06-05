using FoodDeliverySystem.Common.ApiResponse.Models;

namespace FoodDeliverySystem.Common.ApiResponse.Interfaces;

public interface IApiResponseBuilder
{
    IApiResponseBuilder WithStatus(string status);
    IApiResponseBuilder WithMessage(string message);
    IApiResponseBuilder WithData<T>(T? data);
    IApiResponseBuilder WithErrors(IEnumerable<ErrorDetail> errors);
    IApiResponseBuilder AddError(string code, string message);
    IApiResponse Build();
}
