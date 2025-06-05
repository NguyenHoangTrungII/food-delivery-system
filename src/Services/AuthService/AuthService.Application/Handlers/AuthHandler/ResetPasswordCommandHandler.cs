using AuthService.Application.Commands.Auth;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.IRepositories;
using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using BCrypt.Net;

namespace AuthService.Application.CommandHandlers.Auth;

public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommand, ApiResponseWithData<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public ResetPasswordCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponseWithData<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Kiểm tra user tồn tại
            var user = await _unitOfWork.Repository.FirstOrDefaultAsync<User>(u => u.Email == request.Email);
            if (user == null)
            {
                var errors = new[] { new ErrorDetail("NOT_FOUND", $"User with email {request.Email} not found.") };
                return ResponseFactory<string>.NotFound("User not found.", errors);
            }

            // 2. Kiểm tra token reset
            var resetToken = await _unitOfWork.Repository.FirstOrDefaultAsync<PasswordResetToken>(
                t => t.UserId == user.Id && t.Token == request.Token && !t.IsUsed && t.ExpiresAt > DateTime.UtcNow);
            if (resetToken == null)
            {
                var errors = new[] { new ErrorDetail("INVALID_TOKEN", "Invalid or expired token.") };
                return ResponseFactory<string>.BadRequest("Invalid token.", errors);
            }

            // 3. Kiểm tra password mới không trùng với password cũ
            if (BCrypt.Net.BCrypt.Verify(request.NewPassword, user.PasswordHash))
            {
                var errors = new[] { new ErrorDetail("INVALID_PASSWORD", "New password cannot be the same as the old password.") };
                return ResponseFactory<string>.BadRequest("Invalid password.", errors);
            }

            // 4. Cập nhật password
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            resetToken.IsUsed = true;

            _unitOfWork.Repository.Update(user);
            _unitOfWork.Repository.Update(resetToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitAsync(transaction, cancellationToken);

            // 5. Tạo response
            return ResponseFactory<string>.Ok("Password reset successfully.", "Password has been reset successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(transaction, cancellationToken);
            var errors = new[] { new ErrorDetail("INTERNAL_ERROR", $"An unexpected error occurred: {ex.Message}") };
            return ResponseFactory<string>.InternalServerError("An unexpected error occurred.", errors);
        }
    }
}