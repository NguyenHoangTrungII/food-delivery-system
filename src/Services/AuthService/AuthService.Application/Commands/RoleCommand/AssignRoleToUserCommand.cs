using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;
using AuthService.Application.Dtos;

namespace AuthService.Application.Commands.Auth;

public record AssignRoleToUserCommand(Guid UserId, Guid RoleId, DateTime StartDate) : IRequest<ApiResponseWithData<UserDto>>;