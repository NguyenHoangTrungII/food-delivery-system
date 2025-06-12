using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.ServiceClient.Models;

namespace FoodDeliverySystem.Common.ServiceClient.Interfaces;

public interface IServiceClient
{
    Task<ApiResponseWithData<T>> GetAsync<T>(string endpoint, ServiceClientOptions? options = null, CancellationToken cancellationToken = default);
    Task<ApiResponseWithData<T>> PostAsync<T>(string endpoint, object? content, ServiceClientOptions? options = null, CancellationToken cancellationToken = default);
    Task<ApiResponseWithData<T>> PutAsync<T>(string endpoint, object? content, ServiceClientOptions? options = null, CancellationToken cancellationToken = default);
    Task<ApiResponseWithData<T>> DeleteAsync<T>(string endpoint, ServiceClientOptions? options = null, CancellationToken cancellationToken = default);
}