using AuthService.Application.Commands.Auth;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.IRepositories;
using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthService.Application.CommandHandlers.Auth;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, ApiResponseWithData<string>>
{
    private readonly IUnitOfWork _unitOfWork;

    public LogoutCommandHandler(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
    }

    public async Task<ApiResponseWithData<string>> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Kiểm tra refresh token
            var refreshToken = await _unitOfWork.Repository.FirstOrDefaultAsync<RefreshToken>(
                t => t.Token == request.RefreshToken && !t.IsRevoked && t.ExpiryDate > DateTime.UtcNow);
            if (refreshToken == null)
            {
                var errors = new[] { new ErrorDetail("INVALID_TOKEN", "Invalid or expired refresh token.") };
                return ResponseFactory<string>.BadRequest("Invalid token.", errors);
            }

            // 2. Vô hiệu hóa refresh token
            refreshToken.IsRevoked = true;
            _unitOfWork.Repository.Update(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitAsync(transaction, cancellationToken);

            // 3. Tạo response
            return ResponseFactory<string>.Ok("Logged out successfully.", "User has been logged out.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(transaction, cancellationToken);
            var errors = new[] { new ErrorDetail("INTERNAL_ERROR", $"An unexpected error occurred: {ex.Message}") };
            return ResponseFactory<string>.InternalServerError("An unexpected error occurred.", errors);
        }
    }
}