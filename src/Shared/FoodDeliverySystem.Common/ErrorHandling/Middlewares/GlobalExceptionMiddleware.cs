using FoodDeliverySystem.Common.Responses;
using FoodDeliverySystem.Common.Exceptions;
using FoodDeliverySystem.Common.ErrorCodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;

namespace FoodDeliverySystem.Common.Middlewares;

/// <summary>
/// Middleware to handle exceptions globally, returning standardized ApiResponse.
/// Requires UseRequestLocalization in the application pipeline to support multi-language error messages.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IStringLocalizer _localizer;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IStringLocalizer localizer)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _localizer = localizer ?? throw new ArgumentNullException(nameof(localizer));
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        ApiResponse<object> apiResponse;

        switch (exception)
        {
            case ValidationException valEx:
                response.StatusCode = valEx.StatusCode;
                apiResponse = ResponseFactory<object>.ValidationError(valEx.ErrorDetails, _localizer["VALIDATION_ERROR"].Value);
                _logger.LogWarning(exception, "Validation error: {ErrorCode} - {Message}", valEx.ErrorCode, ErrorCodeMap.GetDefaultMessage(valEx.ErrorCode));
                break;

            case NotFoundException notFoundEx:
                response.StatusCode = notFoundEx.StatusCode;
                apiResponse = ResponseFactory<object>.NotFound(_localizer["NOT_FOUND", notFoundEx.Message.Split(" ")[0], notFoundEx.Message.Split(" ")[3]].Value);
                _logger.LogWarning(exception, "Not found error: {ErrorCode} - {Message}", notFoundEx.ErrorCode, ErrorCodeMap.GetDefaultMessage(notFoundEx.ErrorCode));
                break;

            case UnauthorizedException unauthEx:
                response.StatusCode = unauthEx.StatusCode;
                apiResponse = ResponseFactory<object>.Unauthorized(_localizer["UNAUTHORIZED"].Value);
                _logger.LogWarning(exception, "Unauthorized error: {ErrorCode} - {Message}", unauthEx.ErrorCode, ErrorCodeMap.GetDefaultMessage(unauthEx.ErrorCode));
                break;

            case ForbiddenException forbiddenEx:
                response.StatusCode = forbiddenEx.StatusCode;
                apiResponse = ResponseFactory<object>.Forbidden(_localizer["FORBIDDEN"].Value);
                _logger.LogWarning(exception, "Forbidden error: {ErrorCode} - {Message}", forbiddenEx.ErrorCode, ErrorCodeMap.GetDefaultMessage(forbiddenEx.ErrorCode));
                break;

            //case AuthException authEx:
            //    response.StatusCode = authEx.StatusCode;
            //    apiResponse = ResponseFactory<object>.BadRequest(
            //        _localizer["AUTH_ERROR"].Value,
            //        new[] { ErrorCodeMap.CreateErrorDetail(_localizer, "AUTH_ERROR", "", authEx.Message) }
            //    );
            //    _logger.LogWarning(exception, "Authentication error: {Message}", ErrorCodeMap.GetDefaultMessage("AUTH_ERROR"));
            //    break;

            default:
                response.StatusCode = StatusCodes.Status500InternalServerError;
                apiResponse = ResponseFactory<object>.InternalServerError(_localizer["INTERNAL_SERVER_ERROR"].Value);
                _logger.LogError(exception, "An unhandled exception occurred.");
                break;
        }

        var result = JsonSerializer.Serialize(apiResponse, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        await response.WriteAsync(result);
    }
}

public static class GlobalExceptionMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<GlobalExceptionMiddleware>();
    }
}