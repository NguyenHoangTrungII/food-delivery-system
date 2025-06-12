using System.Text.Json.Serialization;

namespace FoodDeliverySystem.Common.ApiResponse.Models;

public class ApiResponseWithData<T> : ApiResponse
{
    public T? Data { get; set; }

    public ApiResponseWithData(string status, string message, T? data, IEnumerable<ErrorDetail>? errors = null)
        : base(status, message, errors)
    {
        Data = data;
    }

    public ApiResponseWithData()
       : base()
    {
    }

}