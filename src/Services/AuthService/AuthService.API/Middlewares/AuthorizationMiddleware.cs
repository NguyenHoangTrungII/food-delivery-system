//using AuthService.Application.Queries.CheckUserPermission;
//using MassTransit.Mediator;
//using MediatR;
//using Microsoft.AspNetCore.Http;
//using System.Security.Claims;
//using IMediator = MediatR.IMediator;

//namespace AuthService.API.Middlewares;

//public class AuthorizationMiddleware
//{
//    private readonly RequestDelegate _next;
//    private readonly IServiceProvider _serviceProvider;

//    public AuthorizationMiddleware(RequestDelegate next, IServiceProvider serviceProvider)
//    {
//        _next = next;
//        _serviceProvider = serviceProvider;
//    }

//    public async Task InvokeAsync(HttpContext context)
//    {
//        var endpoint = context.GetEndpoint();
//        var requiredPermission = endpoint?.Metadata.GetMetadata<RequiredPermissionAttribute>();

//        if (requiredPermission != null)
//        {
//            var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
//            if (string.IsNullOrEmpty(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
//            {
//                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
//                await context.Response.WriteAsync("Unauthorized: Invalid or missing user ID.");
//                return;
//            }

//            using var scope = _serviceProvider.CreateScope();
//            var mediator = scope.ServiceProvider.GetRequiredService<IMediator>();

//            var hasPermission = await mediator.Send(new CheckUserPermissionQuery(userId, requiredPermission.FunctionId));
//            if (!hasPermission)
//            {
//                context.Response.StatusCode = StatusCodes.Status403Forbidden;
//                await context.Response.WriteAsync($"Forbidden: User does not have permission to perform {requiredPermission.FunctionId}.");
//                return;
//            }
//        }

//        await _next(context);
//    }
//}

//public class RequiredPermissionAttribute : Attribute
//{
//    public Guid FunctionId { get; }

//    public RequiredPermissionAttribute(Guid functionId)
//    {
//        FunctionId = functionId;
//    }
//}

//public static class AuthorizationMiddlewareExtensions
//{
//    public static IApplicationBuilder UseCustomAuthorization(this IApplicationBuilder builder)
//    {
//        return builder.UseMiddleware<AuthorizationMiddleware>();
//    }
//}