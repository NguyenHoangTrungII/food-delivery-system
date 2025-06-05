using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;

namespace AuthService.Application.Commands.Auth;

public record ResetPasswordCommand(string Email, string Token, string NewPassword) : IRequest<ApiResponseWithData<string>>;