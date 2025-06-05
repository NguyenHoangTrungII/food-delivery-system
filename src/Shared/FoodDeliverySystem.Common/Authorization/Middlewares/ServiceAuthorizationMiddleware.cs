using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Authorization.Attributes;
using FoodDeliverySystem.Common.Authorization.Models;
using FoodDeliverySystem.Common.Authorization.Interfaces;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using System.Text.Json;

namespace FoodDeliverySystem.Common.Authorization.Middlewares;

public class ServiceAuthorizationMiddleware
{
    private readonly RequestDelegate _next;

    public ServiceAuthorizationMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IPermissionChecker permissionChecker)
    {
        var endpoint = context.GetEndpoint();
        var requiredPermission = endpoint?.Metadata.GetMetadata<RequiredPermissionAttribute>();

        if (requiredPermission != null)
        {
            Guid userId;
            if (context.Request.Headers.TryGetValue("X-Permission-UserId", out var userIdHeader) &&
                Guid.TryParse(userIdHeader, out userId))
            {
                // UserId từ header
            }
            else
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out userId))
                {
                    context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                    var response = ResponseFactory<object>.Unauthorized("Unauthorized: Invalid or missing user ID.");
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    return;
                }
            }

            var request = new PermissionCheckRequest
            {
                UserId = userId,
                CodeName = requiredPermission.CodeName,
            };

            try
            {
                var hasPermission = await permissionChecker.HasPermissionAsync(request);
                if (!hasPermission) // Sửa để dùng ApiResponseWithData<bool>
                {
                    context.Response.StatusCode = StatusCodes.Status403Forbidden;
                    var response = ResponseFactory<object>.Forbidden($"Forbidden: User does not have permission for {requiredPermission.CodeName}.");
                    await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                    return;
                }
            }
            catch (Exception ex)
            {
                context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                var response = ResponseFactory<object>.InternalServerError("Error checking permission.", new[] { new ErrorDetail("PERMISSION_CHECK_ERROR", ex.Message) });
                await context.Response.WriteAsync(JsonSerializer.Serialize(response));
                return;
            }
        }

        await _next(context);
    }
}

public static class ServiceAuthorizationMiddlewareExtensions
{
    public static IApplicationBuilder UseServiceAuthorization(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ServiceAuthorizationMiddleware>();
    }
}
