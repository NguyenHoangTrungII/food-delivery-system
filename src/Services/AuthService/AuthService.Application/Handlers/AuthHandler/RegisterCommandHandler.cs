using AuthService.Application.Commands.Auth;
using AuthService.Application.Dtos;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.IRepositories;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using BCrypt.Net;
using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;

namespace AuthService.Application.CommandHandlers.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponseWithData<UserDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private const string DefaultCustomerRoleName = "Customer";

    public RegisterCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponseWithData<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Kiểm tra username hoặc email đã tồn tại
            var existingUser = await _unitOfWork.Repository.FirstOrDefaultAsync<User>(
                u => u.Username == request.Username || u.Email == request.Email);
            if (existingUser != null)
            {
                var errors = new[] { new ErrorDetail("CONFLICT", "Username or email already exists.") };
                return ResponseFactory<UserDto>.BadRequest("Registration failed.", errors);
            }

            // 2. Tạo user mới
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = request.Username,
                Email = request.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };

            var customerRole = await _unitOfWork.Repository.FirstOrDefaultAsync<Role>(
                r => r.Name == DefaultCustomerRoleName);
            if (customerRole == null)
            {
                customerRole = new Role
                {
                    Id = Guid.NewGuid(),
                    Name = DefaultCustomerRoleName,
                    Administrator = false,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = "system"
                };
                _unitOfWork.Repository.Add(customerRole);
            }

            // 4. Gán role "Customer" cho user
            var userRole = new UserRole
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                RoleId = customerRole.Id,
                StartDate = DateTime.UtcNow,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "system"
            };
            user.UserRoles.Add(userRole);

            _unitOfWork.Repository.Add(user);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitAsync(transaction, cancellationToken);

            // 3. Tạo response
            var responseDto = new UserDto
            {
                Id = user.Id,
                Username = user.Username,
                Email = user.Email,
                Roles = new List<RoleDto>()
            };


            return ResponseFactory<UserDto>.Created(responseDto, "User registered successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(transaction, cancellationToken);
            var errors = new[] { new ErrorDetail("INTERNAL_ERROR", $"An unexpected error occurred: {ex.Message}") };
            return ResponseFactory<UserDto>.InternalServerError("An unexpected error occurred.", errors);
        }
    }
}