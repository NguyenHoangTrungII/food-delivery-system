using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;
using AuthService.Application.Dtos;

namespace AuthService.Application.Commands.Auth;

public record RegisterCommand(string Username, string Email, string Password) : IRequest<ApiResponseWithData<UserDto>>;