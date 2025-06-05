using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;

namespace AuthService.Application.Commands.Auth;

public record LogoutCommand(string RefreshToken) : IRequest<ApiResponseWithData<string>>;