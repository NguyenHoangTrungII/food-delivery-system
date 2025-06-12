using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.ServiceClient.Configuration;
using FoodDeliverySystem.Common.ServiceClient.Interfaces;
using FoodDeliverySystem.Common.ServiceClient.Models;
using FoodDeliverySystem.Common.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using FoodDeliverySystem.Common.ApiResponse.Factories.Extensions;

namespace FoodDeliverySystem.Common.ServiceClient.Implementations;

public class ServiceClient : IServiceClient
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ServiceClientConfig _config;
    private readonly ILoggerAdapter<ServiceClient> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        IgnoreNullValues = true,
        IncludeFields = true
    };

    public ServiceClient(
        IHttpClientFactory httpClientFactory,
        IOptions<ServiceClientConfig> config,
        ILoggerAdapter<ServiceClient> logger,
        IHttpContextAccessor httpContextAccessor)
    {
        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
    }

    public async Task<ApiResponseWithData<T>> GetAsync<T>(string endpoint, ServiceClientOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteRequestAsync<T>(HttpMethod.Get, endpoint, null, options, cancellationToken);
    }

    public async Task<ApiResponseWithData<T>> PostAsync<T>(string endpoint, object? content, ServiceClientOptions? options = null, CancellationToken cancellationToken = default)
    {
        var httpContent = content != null
            ? new StringContent(JsonSerializer.Serialize(content, _jsonOptions), Encoding.UTF8, "application/json")
            : null;
        return await ExecuteRequestAsync<T>(HttpMethod.Post, endpoint, httpContent, options, cancellationToken);
    }

    public async Task<ApiResponseWithData<T>> PutAsync<T>(string endpoint, object? content, ServiceClientOptions? options = null, CancellationToken cancellationToken = default)
    {
        var httpContent = content != null
            ? new StringContent(JsonSerializer.Serialize(content, _jsonOptions), Encoding.UTF8, "application/json")
            : null;
        return await ExecuteRequestAsync<T>(HttpMethod.Put, endpoint, httpContent, options, cancellationToken);
    }

    public async Task<ApiResponseWithData<T>> DeleteAsync<T>(string endpoint, ServiceClientOptions? options = null, CancellationToken cancellationToken = default)
    {
        return await ExecuteRequestAsync<T>(HttpMethod.Delete, endpoint, null, options, cancellationToken);
    }

    private async Task<ApiResponseWithData<T>> ExecuteRequestAsync<T>(
        HttpMethod method,
        string endpoint,
        HttpContent? content,
        ServiceClientOptions? options,
        CancellationToken cancellationToken)
    {
        int attempt = 0;
        var httpClient = _httpClientFactory.CreateClient();
        httpClient.BaseAddress = new Uri(_config.BaseAddress);
        httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

        while (attempt < _config.MaxRetryAttempts)
        {
            try
            {
                _logger.LogInformation("Executing {Method} request to {Endpoint}, attempt {Attempt}", method, endpoint, attempt + 1);

                using var request = new HttpRequestMessage(method, endpoint);
                if (content != null)
                {
                    request.Content = content;
                }

                var headers = options?.Headers != null ? new Dictionary<string, string>(options.Headers) : new Dictionary<string, string>();

                // Ưu tiên X-Permission-UserId từ options, nếu không có thì lấy từ HttpContext
                if (!headers.ContainsKey("X-Permission-UserId"))
                {
                    var userId = GetUserIdFromHttpContext();
                    if (userId.HasValue && userId != Guid.Empty)
                    {
                        headers["X-Permission-UserId"] = userId.Value.ToString();
                        _logger.LogDebug("Added X-Permission-UserId header with value {UserId} from HttpContext", userId);
                    }
                    else
                    {
                        _logger.LogDebug("Skipped X-Permission-UserId header as no valid UserId found in HttpContext");
                    }
                }
                else
                {
                    _logger.LogDebug("Using X-Permission-UserId from provided headers: {UserId}", headers["X-Permission-UserId"]);
                }

                foreach (var header in headers)
                {
                    request.Headers.Add(header.Key, header.Value);
                }

                if (options?.Timeout.HasValue == true)
                {
                    httpClient.Timeout = options.Timeout.Value;
                }

                var response = await httpClient.SendAsync(request, cancellationToken);
                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (!response.IsSuccessStatusCode)
                {
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<ApiResponseWithData<object>>(responseContent, _jsonOptions);
                        var errorDetails = errorResponse?.Errors ?? new[] { new ErrorDetail(response.StatusCode.ToString(), errorResponse?.Message ?? responseContent) };
                        _logger.LogWarning("Request to {Endpoint} failed with status {StatusCode}: {Message}", endpoint, response.StatusCode, errorResponse?.Message ?? responseContent);
                        return new ResponseFactory<T>().CreateFromStatusCode(
                            (int)response.StatusCode,
                            errorResponse?.Message ?? "Request failed.",
                            errorDetails
                        );
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "Failed to deserialize error response from {Endpoint}. Content: {Content}", endpoint, responseContent);
                        var errorDetail = new ErrorDetail(response.StatusCode.ToString(), $"Failed to deserialize response: {ex.Message}");
                        return new ResponseFactory<T>().CreateFromStatusCode(
                            (int)response.StatusCode,
                            "Failed to process error response.",
                            new[] { errorDetail }
                        );
                    }
                }

                var data = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
                _logger.LogInformation("Request to {Endpoint} succeeded", endpoint);
                return ResponseFactory<T>.Ok(data, "Request successful.");
            }
            catch (HttpRequestException ex)
            {
                attempt++;
                _logger.LogWarning(ex.ToString(), "Request to {Endpoint} failed, attempt {Attempt} of {MaxAttempts}", endpoint, attempt, _config.MaxRetryAttempts);
                if (attempt >= _config.MaxRetryAttempts)
                {
                    _logger.LogError(ex, "Max retry attempts reached for {Endpoint}", endpoint);
                    return ResponseFactory<T>.InternalServerError("Request failed after maximum retries.", new[] { new ErrorDetail("SERVICE_ERROR", ex.Message) });
                }
                await Task.Delay(_config.RetryDelayMilliseconds, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during request to {Endpoint}", endpoint);
                return ResponseFactory<T>.InternalServerError("An unexpected error occurred.", new[] { new ErrorDetail("UNEXPECTED_ERROR", ex.Message) });
            }
            finally
            {
                if (options?.Timeout.HasValue == true)
                {
                    httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
                }
            }
        }

        return ResponseFactory<T>.InternalServerError("Request failed unexpectedly.");
    }

    private Guid? GetUserIdFromHttpContext()
    {
        if (_httpContextAccessor.HttpContext == null)
        {
            _logger.LogDebug("HttpContext is null, skipping UserId extraction");
            return null;
        }

        var user = _httpContextAccessor.HttpContext.User;
        if (user == null || !user.Identity.IsAuthenticated)
        {
            _logger.LogDebug("No authenticated user found in HttpContext");
            return null;
        }

        // Thử lấy claim "sub" hoặc "userId" (tùy cấu hình JWT)
        var userIdClaim = user.FindFirst("sub")?.Value ?? user.FindFirst("userId")?.Value;
        if (Guid.TryParse(userIdClaim, out var userId))
        {
            return userId;
        }

        _logger.LogWarning("UserId claim not found or invalid in HttpContext. Claims: {Claims}",
            string.Join(", ", user.Claims.Select(c => $"{c.Type}: {c.Value}")));
        return null;
    }
}

//using System.Net.Http;
//using System.Net.Http.Json;
//using System.Text;
//using System.Text.Json;
//using FoodDeliverySystem.Common.ApiResponse.Factories;
//using FoodDeliverySystem.Common.ApiResponse.Factories.Extensions;
//using FoodDeliverySystem.Common.ApiResponse.Models;
//using FoodDeliverySystem.Common.ServiceClient.Configuration;
//using FoodDeliverySystem.Common.ServiceClient.Interfaces;
//using FoodDeliverySystem.Common.ServiceClient.Models;
//using FoodDeliverySystem.Common.Logging;
//using Microsoft.Extensions.Options;
//using FoodDeliverySystem.Common.ApiResponse.Interfaces;

//namespace FoodDeliverySystem.Common.ServiceClient.Implementations;

//public class ServiceClient : IServiceClient
//{
//    private readonly IHttpClientFactory _httpClientFactory;
//    private readonly ServiceClientConfig _config;
//    private readonly ILoggerAdapter<ServiceClient> _logger;
//    private static readonly JsonSerializerOptions _jsonOptions = new()
//    {
//        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
//        IgnoreNullValues = true
//    };

//    public ServiceClient(IHttpClientFactory httpClientFactory, IOptions<ServiceClientConfig> config, ILoggerAdapter<ServiceClient> logger)
//    {
//        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
//        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }

//    public async Task<ApiResponseWithData<T>> GetAsync<T>(string endpoint, ServiceClientOptions? options = null, CancellationToken cancellationToken = default)
//    {
//        return await ExecuteRequestAsync<T>(HttpMethod.Get, endpoint, null, options, cancellationToken);
//    }

//    public async Task<ApiResponseWithData<T>> PostAsync<T>(string endpoint, object? content, ServiceClientOptions? options = null, CancellationToken cancellationToken = default)
//    {
//        var httpContent = content != null
//            ? new StringContent(JsonSerializer.Serialize(content, _jsonOptions), Encoding.UTF8, "application/json")
//            : null;
//        return await ExecuteRequestAsync<T>(HttpMethod.Post, endpoint, httpContent, options, cancellationToken);
//    }

//    public async Task<ApiResponseWithData<T>> PutAsync<T>(string endpoint, object? content, ServiceClientOptions? options = null, CancellationToken cancellationToken = default)
//    {
//        var httpContent = content != null
//            ? new StringContent(JsonSerializer.Serialize(content, _jsonOptions), Encoding.UTF8, "application/json")
//            : null;
//        return await ExecuteRequestAsync<T>(HttpMethod.Put, endpoint, httpContent, options, cancellationToken);
//    }

//    public async Task<ApiResponseWithData<T>> DeleteAsync<T>(string endpoint, ServiceClientOptions? options = null, CancellationToken cancellationToken = default)
//    {
//        return await ExecuteRequestAsync<T>(HttpMethod.Delete, endpoint, null, options, cancellationToken);
//    }

//    private async Task<ApiResponseWithData<T>> ExecuteRequestAsync<T>(
//        HttpMethod method,
//        string endpoint,
//        HttpContent? content,
//        ServiceClientOptions? options,
//        CancellationToken cancellationToken)
//    {
//        int attempt = 0;
//        var httpClient = _httpClientFactory.CreateClient();
//        httpClient.BaseAddress = new Uri(_config.BaseAddress);
//        httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

//        while (attempt < _config.MaxRetryAttempts)
//        {
//            try
//            {
//                _logger.LogInformation("Executing {Method} request to {Endpoint}, attempt {Attempt}", method, endpoint, attempt + 1);

//                using var request = new HttpRequestMessage(method, endpoint);
//                if (content != null)
//                {
//                    request.Content = content;
//                }

//                if (options?.Headers != null)
//                {
//                    foreach (var header in options.Headers)
//                    {
//                        request.Headers.Add(header.Key, header.Value);
//                    }
//                }

//                if (options?.Timeout.HasValue == true)
//                {
//                    httpClient.Timeout = options.Timeout.Value;
//                }

//                var response = await httpClient.SendAsync(request, cancellationToken);
//                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

//                if (!response.IsSuccessStatusCode)
//                {
//                    try
//                    {
//                        // Thử phân tích phản hồi lỗi thành ApiResponseWithData<object>
//                        var errorResponse = JsonSerializer.Deserialize<ApiResponseWithData<object>>(responseContent, _jsonOptions);
//                        var errorDetails = errorResponse?.Errors ?? new[] { new ErrorDetail(response.StatusCode.ToString(), errorResponse?.Message ?? responseContent) };
//                        _logger.LogWarning("Request to {Endpoint} failed with status {StatusCode}: {Message}", endpoint, response.StatusCode, errorResponse?.Message ?? responseContent);
//                        return new ResponseFactory<T>().CreateFromStatusCode(
//                            (int)response.StatusCode,
//                            errorResponse?.Message ?? "Request failed.",
//                            errorDetails
//                        );
//                    }
//                    catch (JsonException ex)
//                    {
//                        // Nếu không thể phân tích JSON, trả về lỗi với nội dung thô
//                        _logger.LogWarning(ex.ToString(), "Failed to deserialize error response from {Endpoint}: {Content}", endpoint, responseContent);
//                        var errorDetail = new ErrorDetail(response.StatusCode.ToString(), responseContent);
//                        return new ResponseFactory<T>().CreateFromStatusCode(
//                            (int)response.StatusCode,
//                            "Failed to process error response.",
//                            new[] { errorDetail }
//                        );
//                    }
//                }

//                var data = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
//                _logger.LogInformation("Request to {Endpoint} succeeded", endpoint);
//                return ResponseFactory<T>.Ok(data, "Request successful.");
//            }
//            catch (HttpRequestException ex)
//            {
//                attempt++;
//                _logger.LogWarning(ex.ToString(), "Request to {Endpoint} failed, attempt {Attempt} of {MaxAttempts}", endpoint, attempt, _config.MaxRetryAttempts);
//                if (attempt >= _config.MaxRetryAttempts)
//                {
//                    _logger.LogError(ex, "Max retry attempts reached for {Endpoint}", endpoint);
//                    return ResponseFactory<T>.InternalServerError("Request failed after maximum retries.", new[] { new ErrorDetail("SERVICE_ERROR", ex.Message) });
//                }
//                await Task.Delay(_config.RetryDelayMilliseconds, cancellationToken);
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Unexpected error during request to {Endpoint}", endpoint);
//                return ResponseFactory<T>.InternalServerError("An unexpected error occurred.", new[] { new ErrorDetail("UNEXPECTED_ERROR", ex.Message) });
//            }
//            finally
//            {
//                if (options?.Timeout.HasValue == true)
//                {
//                    httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
//                }
//            }
//        }

//        return ResponseFactory<T>.InternalServerError("Request failed unexpectedly.");
//    }
//}

////using System.Net.Http;
////using System.Net.Http.Json;
////using System.Text;
////using System.Text.Json;
////using FoodDeliverySystem.Common.ApiResponse.Factories;
////using FoodDeliverySystem.Common.ApiResponse.Factories.Extensions;
////using FoodDeliverySystem.Common.ApiResponse.Models;
////using FoodDeliverySystem.Common.ServiceClient.Configuration;
////using FoodDeliverySystem.Common.ServiceClient.Interfaces;
////using FoodDeliverySystem.Common.ServiceClient.Models;
////using FoodDeliverySystem.Common.Logging;
////using Microsoft.Extensions.Options;
////using FoodDeliverySystem.Common.ApiResponse.Interfaces;

////namespace FoodDeliverySystem.Common.ServiceClient.Implementations;

////public class ServiceClient : IServiceClient
////{
////    private readonly IHttpClientFactory _httpClientFactory;
////    private readonly ServiceClientConfig _config;
////    private readonly ILoggerAdapter<ServiceClient> _logger;
////    private static readonly JsonSerializerOptions _jsonOptions = new()
////    {
////        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
////        IgnoreNullValues = true
////    };

////    public ServiceClient(IHttpClientFactory httpClientFactory, IOptions<ServiceClientConfig> config, ILoggerAdapter<ServiceClient> logger)
////    {
////        _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
////        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
////        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
////    }

////    public async Task<ApiResponseWithData<T>> GetAsync<T>(string endpoint, ServiceClientOptions? options = null, CancellationToken cancellationToken = default)
////    {
////        return await ExecuteRequestAsync<T>(HttpMethod.Get, endpoint, null, options, cancellationToken);
////    }

////    public async Task<ApiResponseWithData<T>> PostAsync<T>(string endpoint, object? content, ServiceClientOptions? options = null, CancellationToken cancellationToken = default)
////    {
////        var httpContent = content != null
////            ? new StringContent(JsonSerializer.Serialize(content, _jsonOptions), Encoding.UTF8, "application/json")
////            : null;
////        return await ExecuteRequestAsync<T>(HttpMethod.Post, endpoint, httpContent, options, cancellationToken);
////    }

////    public async Task<ApiResponseWithData<T>> PutAsync<T>(string endpoint, object? content, ServiceClientOptions? options = null, CancellationToken cancellationToken = default)
////    {
////        var httpContent = content != null
////            ? new StringContent(JsonSerializer.Serialize(content, _jsonOptions), Encoding.UTF8, "application/json")
////            : null;
////        return await ExecuteRequestAsync<T>(HttpMethod.Put, endpoint, httpContent, options, cancellationToken);
////    }

////    public async Task<ApiResponseWithData<T>> DeleteAsync<T>(string endpoint, ServiceClientOptions? options = null, CancellationToken cancellationToken = default)
////    {
////        return await ExecuteRequestAsync<T>(HttpMethod.Delete, endpoint, null, options, cancellationToken);
////    }

////    private async Task<ApiResponseWithData<T>> ExecuteRequestAsync<T>(
////        HttpMethod method,
////        string endpoint,
////        HttpContent? content,
////        ServiceClientOptions? options,
////        CancellationToken cancellationToken)
////    {
////        int attempt = 0;
////        var httpClient = _httpClientFactory.CreateClient();
////        httpClient.BaseAddress = new Uri(_config.BaseAddress);
////        httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);

////        while (attempt < _config.MaxRetryAttempts)
////        {
////            try
////            {
////                _logger.LogInformation("Executing {Method} request to {Endpoint}, attempt {Attempt}", method, endpoint, attempt + 1);

////                using var request = new HttpRequestMessage(method, endpoint);
////                if (content != null)
////                {
////                    request.Content = content;
////                }

////                if (options?.Headers != null)
////                {
////                    foreach (var header in options.Headers)
////                    {
////                        request.Headers.Add(header.Key, header.Value);
////                    }
////                }

////                if (options?.Timeout.HasValue == true)
////                {
////                    httpClient.Timeout = options.Timeout.Value;
////                }

////                var response = await httpClient.SendAsync(request, cancellationToken);
////                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

////                if (!response.IsSuccessStatusCode)
////                {
////                    var errorDetail = new ErrorDetail(response.StatusCode.ToString(), responseContent);
////                    return new ResponseFactory<T>().CreateFromStatusCode((int)response.StatusCode, responseContent, new[] { errorDetail });
////                }

////                var data = JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
////                _logger.LogInformation("Request to {Endpoint} succeeded", endpoint);
////                return ResponseFactory<T>.Ok(data, "Request successful.");
////            }
////            catch (HttpRequestException ex)
////            {
////                attempt++;
////                _logger.LogWarning(ex.ToString(), "Request to {Endpoint} failed, attempt {Attempt} of {MaxAttempts}", endpoint, attempt, _config.MaxRetryAttempts);
////                if (attempt >= _config.MaxRetryAttempts)
////                {
////                    _logger.LogError(ex, "Max retry attempts reached for {Endpoint}", endpoint);
////                    return ResponseFactory<T>.InternalServerError("Request failed after maximum retries.", new[] { new ErrorDetail("SERVICE_ERROR", ex.Message) });
////                }
////                await Task.Delay(_config.RetryDelayMilliseconds, cancellationToken);
////            }
////            catch (Exception ex)
////            {
////                _logger.LogError(ex, "Unexpected error during request to {Endpoint}", endpoint);
////                return ResponseFactory<T>.InternalServerError("An unexpected error occurred.", new[] { new ErrorDetail("UNEXPECTED_ERROR", ex.Message) });
////            }
////            finally
////            {
////                if (options?.Timeout.HasValue == true)
////                {
////                    httpClient.Timeout = TimeSpan.FromSeconds(_config.TimeoutSeconds);
////                }
////            }
////        }

////        return ResponseFactory<T>.InternalServerError("Request failed unexpectedly.");
////    }
////}