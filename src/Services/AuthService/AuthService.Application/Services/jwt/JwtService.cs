using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.IRepositories;
using System.Security.Cryptography;

namespace AuthService.Application.Services;

public class JwtService : IJwtService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly string _jwtSecret;
    private readonly int _jwtExpiryMinutes;
    private readonly string _issuer;
    private readonly string _audience;

    public JwtService(IUnitOfWork unitOfWork, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _jwtSecret = configuration["Jwt:Secret"] ?? throw new ArgumentException("Jwt:Secret is missing in configuration.", nameof(configuration));
        _jwtExpiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var expiry) ? expiry : 60;
        _issuer = configuration["Jwt:Issuer"] ?? "AuthService";
        _audience = configuration["Jwt:Audience"] ?? "FoodDeliverySystem";
    }

    public async Task<ApiResponseWithData<string>> GenerateJwtTokenAsync(User user)
    {
        if (user == null)
        {
            var errors = new[] { new ErrorDetail("VALIDATION_USER_NULL", "Thông tin người dùng không được để trống.") };
            return ResponseFactory<string>.BadRequest("User cannot be null.", errors);
        }

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSecret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _issuer,
            audience: _audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes),
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return ResponseFactory<string>.Ok(tokenString, "Token generated successfully.");
    }

    public async Task<ApiResponseWithData<User>> ValidateJwtTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            var errors = new[] { new ErrorDetail("VALIDATION_TOKEN_EMPTY", "Token không được để trống.") };
            return ResponseFactory<User>.BadRequest("Token cannot be empty.", errors);
        }

        try
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_jwtSecret);

            tokenHandler.ValidateToken(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _issuer,
                ValidAudience = _audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.FromSeconds(30)
            }, out SecurityToken validatedToken);

            var jwtToken = (JwtSecurityToken)validatedToken;
            var userIdClaim = jwtToken.Claims.FirstOrDefault(x => x.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !Guid.TryParse(userIdClaim.Value, out var userId))
            {
                var errors = new[] { new ErrorDetail("INVALID_TOKEN", "Invalid user ID in token.") };
                return ResponseFactory<User>.Unauthorized("Invalid token.", errors);
            }

            var user = await _unitOfWork.Repository.GetByIdAsync<User>(userId);
            if (user == null)
            {
                var errors = new[] { new ErrorDetail("USER_NOT_FOUND", $"User with ID {userId} not found.") };
                return ResponseFactory<User>.NotFound("User not found.", errors);
            }

            return ResponseFactory<User>.Ok(user, "Token validated successfully.");
        }
        catch (SecurityTokenException)
        {
            var errors = new[] { new ErrorDetail("INVALID_TOKEN", "Invalid JWT token.") };
            return ResponseFactory<User>.Unauthorized("Invalid token.", errors);
        }
        catch (Exception ex)
        {
            var errors = new[] { new ErrorDetail("INTERNAL_ERROR", $"Error validating token: {ex.Message}") };
            return ResponseFactory<User>.InternalServerError("An unexpected error occurred.", errors);
        }


    }

    public  RefreshToken GenerateRefreshToken(User user)
    {
        return new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            Token = Guid.NewGuid().ToString("N"),
            ExpiryDate = DateTime.UtcNow.AddDays(10),
            CreatedAt = DateTime.UtcNow,
            CreatedBy = "system",
            IsRevoked = false
        };
    }


    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }


}