using AuthService.Application.Dtos;
using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;

namespace AuthService.Application.Commands.Auth;

public record RefreshTokenCommand(string RefreshToken) : IRequest<ApiResponseWithData<LoginResponseDto>>;