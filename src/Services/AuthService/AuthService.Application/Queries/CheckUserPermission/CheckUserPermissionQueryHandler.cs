using AuthService.Domain.Entities;
using AuthService.Infrastructure.IRepositories;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.Queries.CheckUserPermission;

public class CheckUserPermissionQueryHandler : IRequestHandler<CheckUserPermissionQuery, bool>
{
    private readonly IUnitOfWork _unitOfWork;

    public CheckUserPermissionQueryHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<bool> Handle(CheckUserPermissionQuery request, CancellationToken cancellationToken)
    {
        var user = await _unitOfWork.Repository.GetAll<User>()
            .Include(u => u.UserRoles)
            .ThenInclude(ur => ur.Role)
            .ThenInclude(r => r.RolePermissions)
            .FirstOrDefaultAsync(u => u.Id == request.UserId, cancellationToken);

        if (user == null)
        {
            return false;
        }

        // Kiểm tra quyền từ tất cả vai trò của user
        var hasPermission = user.UserRoles
            .SelectMany(ur => ur.Role.RolePermissions)
            .Any(rp => rp.FunctionID == request.FunctionId && rp.Allowed);

        return hasPermission;
    }
}