using AuthService.Application.Dtos;
using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;

namespace AuthService.Application.Commands.Auth;

public record GoogleLoginCommand(string IdToken) : IRequest<ApiResponseWithData<LoginResponseDto>>;

