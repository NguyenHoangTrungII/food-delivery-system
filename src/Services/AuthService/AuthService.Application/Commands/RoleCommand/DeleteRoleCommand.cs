using MediatR;

namespace AuthService.Application.Commands;

public record DeleteRoleCommand(Guid RoleId) : IRequest;