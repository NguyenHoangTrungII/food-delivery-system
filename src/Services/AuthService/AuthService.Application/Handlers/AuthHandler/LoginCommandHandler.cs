// src/AuthService/Application/CommandHandlers/Auth/LoginCommandHandler.cs
using AuthService.Application.Commands.Auth;
using AuthService.Application.Dtos;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.IRepositories;
using AuthService.Application.Services;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using BCrypt.Net;
using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using Microsoft.Extensions.Configuration;
using AuthService.Application.Services.jwt;
using FoodDeliverySystem.Common.Authorization.Interfaces;
using FoodDeliverySystem.Common.Caching.Configuration;
using FoodDeliverySystem.Common.Caching.Services;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Application.CommandHandlers.Auth;

public class LoginCommandHandler : IRequestHandler<LoginCommand, ApiResponseWithData<LoginResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IRefreshTokenService _refreshTokenService;
    private readonly ICacheService _permissionCacheService; // Thêm IPermissionCacheService
    private readonly CacheOptions _cacheOptions; // Thêm CacheOptions
    private readonly int _jwtExpiryMinutes;

    public LoginCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IRefreshTokenService refreshTokenService,
        ICacheService permissionCacheService, // Inject IPermissionCacheService
        CacheOptions cacheOptions, // Inject CacheOptions
        IConfiguration configuration)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _refreshTokenService = refreshTokenService ?? throw new ArgumentNullException(nameof(refreshTokenService));
        _permissionCacheService = permissionCacheService ?? throw new ArgumentNullException(nameof(permissionCacheService));
        _cacheOptions = cacheOptions ?? throw new ArgumentNullException(nameof(cacheOptions));
        _jwtExpiryMinutes = int.TryParse(configuration["Jwt:ExpiryMinutes"], out var expiry) ? expiry : 60;
    }

    public async Task<ApiResponseWithData<LoginResponseDto>> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {

            var user = await _unitOfWork.Repository.FirstOrDefaultAsync<User>(
             u => u.Username == request.Username,
             include: u => u
                 .Include(u => u.UserRoles)
                     .ThenInclude(ur => ur.Role)
                         .ThenInclude(r => r.RolePermissions)
                             .ThenInclude(rp => rp.FunctionAction), 
             cancellationToken);



            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                var errors = new[] { new ErrorDetail("AUTH_ERROR", "Invalid username or password.") };
                return ResponseFactory<LoginResponseDto>.Unauthorized("Invalid username or password.", errors);
            }

            // 2. Tạo JWT
            var tokenResponse = await _jwtService.GenerateJwtTokenAsync(user);
            if (tokenResponse.Status == "error")
            {
                return ResponseFactory<LoginResponseDto>.BadRequest(tokenResponse.Message, tokenResponse.Errors);
            }

            // 3. Tạo Refresh Token
            var refreshTokenResponse = await _refreshTokenService.GenerateRefreshTokenAsync(user);
            if (refreshTokenResponse.Status == "error")
            {
                return ResponseFactory<LoginResponseDto>.BadRequest(refreshTokenResponse.Message, refreshTokenResponse.Errors);
            }

            // 4. Lưu Refresh Token
            var expiry = DateTime.UtcNow.AddMinutes(_jwtExpiryMinutes);
            _unitOfWork.Repository.Add(refreshTokenResponse.Data);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. Lưu permissions vào Redis
            var permissions = user.UserRoles
                .SelectMany(ur => ur.Role.RolePermissions)
                .Where(rp => rp.Allowed && rp.FunctionAction != null)
                .ToDictionary(
                    rp => rp.FunctionAction.CodeName, 
                    rp => rp.Allowed);

            await _permissionCacheService.HashSetAsync(
              key: $"permissions:{user.Id}",
              values: permissions,
              expiry: _cacheOptions.DefaultTTL,
              cancellationToken: cancellationToken);


            await _unitOfWork.CommitAsync(transaction, cancellationToken);

            // 6. Tạo response
            var responseDto = new LoginResponseDto
            {
                Token = tokenResponse.Data,
                RefreshToken = refreshTokenResponse.Data.Token,
                Expiry = expiry
            };

            return ResponseFactory<LoginResponseDto>.Ok(responseDto, "Login successful.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(transaction, cancellationToken);
            var errors = new[] { new ErrorDetail("INTERNAL_ERROR", $"An unexpected error occurred: {ex.Message}") };
            return ResponseFactory<LoginResponseDto>.InternalServerError("An unexpected error occurred.", errors);
        }
    }
}