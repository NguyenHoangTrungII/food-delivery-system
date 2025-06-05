using AuthService.Application.Commands.Auth;
using AuthService.Application.Dtos;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.IRepositories;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;

namespace AuthService.Application.CommandHandlers.Auth;

public class CreateRoleCommandHandler : IRequestHandler<CreateRoleCommand, ApiResponseWithData<RoleDto>>
{
    private readonly IUnitOfWork _unitOfWork;

    public CreateRoleCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponseWithData<RoleDto>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Kiểm tra role name đã tồn tại
            var existingRole = await _unitOfWork.Repository.FirstOrDefaultAsync<Role>(r => r.Name == request.Name);
            if (existingRole != null)
            {
                var errors = new[] { new ErrorDetail("CONFLICT", $"Role with name {request.Name} already exists.") };
                return ResponseFactory<RoleDto>.BadRequest("Role creation failed.", errors);
            }

            // 2. Tạo role mới
            var role = new Role
            {
                Id = Guid.NewGuid(),
                Name = request.Name,
                Administrator = request.Administrator,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _unitOfWork.Repository.Add(role);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitAsync(transaction, cancellationToken);

            // 3. Tạo response
            var responseDto = new RoleDto
            {
                Id = role.Id,
                Name = role.Name,
                Administrator = role.Administrator
            };

            return ResponseFactory<RoleDto>.Created(responseDto, "Role created successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(transaction, cancellationToken);
            var errors = new[] { new ErrorDetail("INTERNAL_ERROR", $"An unexpected error occurred: {ex.Message}") };
            return ResponseFactory<RoleDto>.InternalServerError("An unexpected error occurred.", errors);
        }
    }
}