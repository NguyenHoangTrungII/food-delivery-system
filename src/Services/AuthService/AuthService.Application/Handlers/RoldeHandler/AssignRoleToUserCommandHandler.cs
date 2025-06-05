using AuthService.Application.Commands.Auth;
using AuthService.Application.Dtos;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.IRepositories;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.CommandHandlers.Auth;

public class AssignRoleToUserCommandHandler : IRequestHandler<AssignRoleToUserCommand, ApiResponseWithData<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public AssignRoleToUserCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponseWithData<UserDto>> Handle(AssignRoleToUserCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Kiểm tra user tồn tại
            var user = await _unitOfWork.Repository.FirstOrDefaultAsync<User>(
                u => u.Id == request.UserId,
                include: q => q.Include(u => u.UserRoles).ThenInclude(ur => ur.Role));
            if (user == null)
            {
                var errors = new[] { new ErrorDetail("NOT_FOUND", $"User with ID {request.UserId} not found.") };
                return ResponseFactory<UserDto>.NotFound("User not found.", errors);
            }

            // 2. Kiểm tra role tồn tại
            var role = await _unitOfWork.Repository.FirstOrDefaultAsync<Role>(r => r.Id == request.RoleId);
            if (role == null)
            {
                var errors = new[] { new ErrorDetail("NOT_FOUND", $"Role with ID {request.RoleId} not found.") };
                return ResponseFactory<UserDto>.NotFound("Role not found.", errors);
            }

            // 3. Kiểm tra user đã có role
            if (user.UserRoles.Any(ur => ur.RoleId == request.RoleId))
            {
                var errors = new[] { new ErrorDetail("CONFLICT", $"User {user.Username} already has role {role.Name}.") };
                return ResponseFactory<UserDto>.BadRequest("Role assignment failed.", errors);
            }

            // 4. Gán role
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = request.UserId,
                RoleId = request.RoleId,
                StartDate = request.StartDate,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _unitOfWork.Repository.Add(userRole);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitAsync(transaction, cancellationToken);

            // 5. Tạo response
            var responseDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = user.UserRoles.Select(ur => new RoleDto
                {
                    Id = ur.Role.Id,
                    Name = ur.Role.Name,
                    Administrator = ur.Role.Administrator
                }).ToList()
            };

            return ResponseFactory<UserDto>.Ok(responseDto, "Role assigned successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(transaction, cancellationToken);
            var errors = new[] { new ErrorDetail("INTERNAL_ERROR", $"An unexpected error occurred: {ex.Message}") };
            return ResponseFactory<UserDto>.InternalServerError("An unexpected error occurred.", errors);
        }
    }
}