using AuthService.Application.Dtos;
using AuthService.Application.Commands.Auth;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.IRepositories;
using AuthService.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Google.Apis.Auth;
using BCrypt.Net;
using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;

namespace AuthService.Application.CommandHandlers.Auth;

public class GoogleLoginCommandHandler : IRequestHandler<GoogleLoginCommand, ApiResponseWithData<LoginResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IConfiguration _configuration;
    private readonly IJwtService _jwtService;
    private readonly int _jwtExpiryMinutes;
    private readonly int _refreshTokenExpiryDays;

    public GoogleLoginCommandHandler(IUnitOfWork unitOfWork, IConfiguration configuration, IJwtService jwtService)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _jwtExpiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var expiry) ? expiry : 60;
        _refreshTokenExpiryDays = int.TryParse(configuration["Jwt:RefreshTokenExpiryDays"], out var days) ? days : 7;
    }

    public async Task<ApiResponseWithData<LoginResponseDto>> Handle(GoogleLoginCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Xác thực ID Token từ Google
            var payloadResult = await ValidateGoogleIdToken(request.IdToken);
            if (payloadResult.Status == "error")
            {
                return ResponseFactory<LoginResponseDto>.BadRequest(payloadResult.Message, payloadResult.Errors);
            }
            var payload = payloadResult.Data;

            // 2. Lấy thông tin user từ payload
            var email = payload.Email;
            var username = email.Split('@')[0]; // Tạo username từ email
            var googleId = payload.Subject;

            // 3. Tìm hoặc tạo user
            var user = await _unitOfWork.Repository.FirstOrDefaultAsync<User>(u => u.Email == email);
            if (user == null)
            {
                user = new User
                {
                    Id = Guid.NewGuid(),
                    Username = username,
                    Email = email,
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString()), // Mật khẩu ngẫu nhiên
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = "google-oauth"
                };
                _unitOfWork.Repository.Add(user);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // 4. Tạo JWT và Refresh Token
            var tokenResponse = await _jwtService.GenerateJwtTokenAsync(user);
            if (tokenResponse.Status == "error")
            {
                return ResponseFactory<LoginResponseDto>.BadRequest(tokenResponse.Message, tokenResponse.Errors);
            }

            var refreshToken = GenerateRefreshToken(user);
            var expiry = DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes);

            _unitOfWork.Repository.Add(refreshToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            await _unitOfWork.CommitAsync(transaction, cancellationToken);

            // 5. Tạo response
            var responseDto = new LoginResponseDto
            {
                Token = tokenResponse.Data,
                RefreshToken = refreshToken.Token,
                Expiry = expiry
            };

            return ResponseFactory<LoginResponseDto>.Ok(responseDto, "Google login successful.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(transaction, cancellationToken);
            var errors = new[] { new ErrorDetail("INTERNAL_ERROR", $"An unexpected error occurred: {ex.Message}") };
            return ResponseFactory<LoginResponseDto>.InternalServerError("An unexpected error occurred.", errors);
        }
    }

    private async Task<ApiResponseWithData<GoogleJsonWebSignature.Payload>> ValidateGoogleIdToken(string idToken)
    {
        if (string.IsNullOrWhiteSpace(idToken))
        {
            var errors = new[] { new ErrorDetail("VALIDATION_TOKEN_EMPTY", "Google ID token cannot be empty.") };
            return ResponseFactory<GoogleJsonWebSignature.Payload>.BadRequest("Invalid token.", errors);
        }

        try
        {
            var settings = new GoogleJsonWebSignature.ValidationSettings
            {
                Audience = new[] { _configuration["GoogleOAuth:ClientId"] }
            };
            var payload = await GoogleJsonWebSignature.ValidateAsync(idToken, settings);
            return ResponseFactory<GoogleJsonWebSignature.Payload>.Ok(payload, "Google token validated successfully.");
        }
        catch (InvalidJwtException)
        {
            var errors = new[] { new ErrorDetail("INVALID_GOOGLE_TOKEN", "Invalid Google ID token.") };
            return ResponseFactory<GoogleJsonWebSignature.Payload>.Unauthorized("Invalid Google token.", errors);
        }
        catch (Exception ex)
        {
            var errors = new[] { new ErrorDetail("INTERNAL_ERROR", $"Error validating Google token: {ex.Message}") };
            return ResponseFactory<GoogleJsonWebSignature.Payload>.InternalServerError("An unexpected error occurred.", errors);
        }
    }

    private RefreshToken GenerateRefreshToken(User user)
    {
        return new RefreshToken
        {
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            UserId = user.Id,
            ExpiryDate = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow
        };
    }
}