using AuthService.Application.Services.jwt;
using AuthService.Domain.Entities;
using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using Microsoft.Extensions.Configuration;

namespace AuthService.Application.Services;

public class RefreshTokenService : IRefreshTokenService
{
    private readonly int _refreshTokenExpiryDays;

    public RefreshTokenService(IConfiguration configuration)
    {
        _refreshTokenExpiryDays = int.TryParse(configuration["Jwt:RefreshTokenExpiryDays"], out var days) ? days : 7;
    }

    public Task<ApiResponseWithData<RefreshToken>> GenerateRefreshTokenAsync(User user)
    {
        if (user == null)
        {
            var errors = new[] { new ErrorDetail("VALIDATION_USER_NULL", "User cannot be null.") };
            return Task.FromResult(ResponseFactory<RefreshToken>.BadRequest("User cannot be null.", errors));
        }

        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Convert.ToBase64String(Guid.NewGuid().ToByteArray()),
            ExpiryDate = DateTime.UtcNow.AddDays(_refreshTokenExpiryDays),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            IsRevoked = false
        };

        return Task.FromResult(ResponseFactory<RefreshToken>.Ok(refreshToken, "Refresh token generated successfully."));
    }
}