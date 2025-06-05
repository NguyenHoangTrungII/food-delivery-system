
using MediatR;

namespace AuthService.Application.Queries.CheckUserPermission;

public record CheckUserPermissionQuery(Guid UserId, Guid FunctionId) : IRequest<bool>;