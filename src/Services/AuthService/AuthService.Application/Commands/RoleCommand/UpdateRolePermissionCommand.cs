using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;
using AuthService.Application.Dtos;

namespace AuthService.Application.Commands.Auth;

public record UpdateRolePermissionCommand(Guid RoleId, Guid  FunctionId, bool Allowed) : IRequest<ApiResponseWithData<RolePermissionDto>>;