using AuthService.Application.Dtos;
using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;

namespace AuthService.Application.Commands.Auth;

public record LoginCommand(string Username, string Password) : IRequest<ApiResponseWithData<LoginResponseDto>>;

