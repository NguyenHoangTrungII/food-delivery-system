using AuthService.Application.Commands.Auth;
using AuthService.Application.Dtos.Auth;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.IRepositories;
using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Email.Interfaces;
using FoodDeliverySystem.Common.Email.Models;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;

namespace AuthService.Application.CommandHandlers.Auth;

public class ForgotPasswordCommandHandler : IRequestHandler<ForgotPasswordCommand, ApiResponseWithData<string>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly string _resetLinkBaseUrl = "https://yourapp.com/reset"; // Configure via appsettings.json

    public ForgotPasswordCommandHandler(IUnitOfWork unitOfWork, IEmailService emailService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _emailService = emailService ?? throw new ArgumentNullException(nameof(emailService));
    }

    public async Task<ApiResponseWithData<string>> Handle(ForgotPasswordCommand request, CancellationToken cancellationToken)
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

            // 2. Tạo token reset password
            var token = Guid.NewGuid().ToString();
            var resetToken = new PasswordResetToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddHours(1), // Hết hạn sau 1 giờ
                IsUsed = false,
                CreatedOn = DateTime.UtcNow,
                CreatedBy = "system"
            };

            _unitOfWork.Repository.Add(resetToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 3. Gửi email với link reset
            var resetLink = $"{_resetLinkBaseUrl}?email={Uri.EscapeDataString(user.Email)}&token={Uri.EscapeDataString(token)}";
            var emailMessage = new EmailMessage(
                toEmail: user.Email,
                toName: user.Username ?? user.Email,
                subject: "Password Reset Request",
                body: $"<h1>Password Reset</h1><p>Click the link below to reset your password:</p><p><a href='{resetLink}'>Reset Password</a></p><p>This link will expire in 1 hour.</p>",
                isHtml: true
            );

            var emailResult = await _emailService.SendEmailAsync(emailMessage, cancellationToken);
            if (emailResult.Status == "error")
            {
                var errors = emailResult.Errors.Select(e => new ErrorDetail(e.Code, e.Message)).ToArray();
                return ResponseFactory<string>.InternalServerError("Failed to send reset email.", errors);
            }

            await _unitOfWork.CommitAsync(transaction, cancellationToken);

            // 4. Tạo response
            return ResponseFactory<string>.Ok("Password reset link sent.", "Password reset link has been sent to your email.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(transaction, cancellationToken);
            var errors = new[] { new ErrorDetail("INTERNAL_ERROR", $"An unexpected error occurred: {ex.Message}") };
            return ResponseFactory<string>.InternalServerError("An unexpected error occurred.", errors);
        }
    }
}