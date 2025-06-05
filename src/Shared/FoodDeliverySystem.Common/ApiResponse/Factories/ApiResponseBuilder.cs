using FoodDeliverySystem.Common.ApiResponse.Interfaces;
using FoodDeliverySystem.Common.ApiResponse.Models;

namespace FoodDeliverySystem.Common.ApiResponse.Factories;

public class ApiResponseBuilder : IApiResponseBuilder
{
    private string _status = "success";
    private string _message = string.Empty;
    private object? _data;
    private readonly List<ErrorDetail> _errors = new();

    public IApiResponseBuilder WithStatus(string status)
    {
        _status = status ?? throw new ArgumentNullException(nameof(status));
        return this;
    }

    public IApiResponseBuilder WithMessage(string message)
    {
        _message = message ?? string.Empty;
        return this;
    }

    public IApiResponseBuilder WithData<T>(T? data)
    {
        _data = data;
        return this;
    }

    public IApiResponseBuilder WithErrors(IEnumerable<ErrorDetail> errors)
    {
        _errors.Clear();
        if (errors != null)
        {
            _errors.AddRange(errors);
        }
        return this;
    }

    public IApiResponseBuilder AddError(string code, string message)
    {
        _errors.Add(new ErrorDetail(code, message));
        return this;
    }

    public IApiResponse Build()
    {
        return new ApiResponseWithData<object>(_status, _message, _data, _errors);
    }
}