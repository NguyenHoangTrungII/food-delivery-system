using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;
using AuthService.Application.Dtos;

namespace AuthService.Application.Commands.Auth;

public record CreateRoleCommand(string Name, bool Administrator) : IRequest<ApiResponseWithData<RoleDto>>;