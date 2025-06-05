using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;

namespace AuthService.Application.Commands.Auth;

public record ForgotPasswordCommand(string Email) : IRequest<ApiResponseWithData<string>>;