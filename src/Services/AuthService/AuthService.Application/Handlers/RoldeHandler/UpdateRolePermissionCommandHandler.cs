using AuthService.Application.Commands.Auth;
using AuthService.Application.Dtos;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.IRepositories;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;

namespace AuthService.Application.CommandHandlers.Auth;

public class UpdateRolePermissionCommandHandler : IRequestHandler<UpdateRolePermissionCommand, ApiResponseWithData<RolePermissionDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public UpdateRolePermissionCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponseWithData<RolePermissionDto>> Handle(UpdateRolePermissionCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Kiểm tra role tồn tại
            var role = await _unitOfWork.Repository.FirstOrDefaultAsync<Role>(r => r.Id == request.RoleId);
            if (role == null)
            {
                var errors = new[] { new ErrorDetail("NOT_FOUND", $"Role with ID {request.RoleId} not found.") };
                return ResponseFactory<RolePermissionDto>.NotFound("Role not found.", errors);
            }

            // 2. Kiểm tra function action tồn tại
            var function = await _unitOfWork.Repository.FirstOrDefaultAsync<FunctionAction>(fa => fa.Id == request.FunctionId);
            if (function == null)
            {
                var errors = new[] { new ErrorDetail("NOT_FOUND", $"Function action with ID {request.FunctionId} not found.") };
                return ResponseFactory<RolePermissionDto>.NotFound("Function action not found.", errors);
            }

            // 3. Tìm hoặc tạo permission
            var permission = await _unitOfWork.Repository.FirstOrDefaultAsync<RolePermission>(
                rp => rp.RoleId == request.RoleId && rp.FunctionID == request.FunctionId);

            if (permission == null)
            {
                permission = new RolePermission
                {
                    Id = Guid.NewGuid(),
                    RoleId = request.RoleId,
                    FunctionID = request.FunctionId,
                    Allowed = request.Allowed,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "system"
                };
                _unitOfWork.Repository.Add(permission);
            }
            else
            {
                permission.Allowed = request.Allowed;
                permission.ModifiedOn = DateTime.UtcNow;
                permission.ModifiedBy = "system";
                _unitOfWork.Repository.Update(permission);
            }

            // 4. Lưu thay đổi
            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(transaction, cancellationToken);

            // 5. Tạo response
            var responseDto = new RolePermissionDto
            {
                Id = permission.Id,
                RoleId = permission.RoleId,
                FunctionID = permission.FunctionID,
                Allowed = permission.Allowed
            };

            return ResponseFactory<RolePermissionDto>.Ok(responseDto, "Role permission updated successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(transaction, cancellationToken);
            var errors = new[] { new ErrorDetail("INTERNAL_ERROR", $"An unexpected error occurred: {ex.Message}") };
            return ResponseFactory<RolePermissionDto>.InternalServerError("An unexpected error occurred.", errors);
        }
    }
}