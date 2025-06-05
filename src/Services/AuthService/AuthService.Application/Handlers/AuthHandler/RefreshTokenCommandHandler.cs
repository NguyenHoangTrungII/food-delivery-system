using AuthService.Application.Commands.Auth;
using AuthService.Application.Dtos;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.IRepositories;
using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using Google.Apis.Auth.OAuth2.Responses;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace AuthService.Application.CommandHandlers.Auth;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, ApiResponseWithData<LoginResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IConfiguration _configuration;

    public RefreshTokenCommandHandler(IUnitOfWork unitOfWork, IJwtService jwtService, IConfiguration configuration)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public async Task<ApiResponseWithData<LoginResponseDto>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            // 1. Kiểm tra refresh token
            var refreshToken = await _unitOfWork.Repository.FirstOrDefaultAsync<RefreshToken>(
                t => t.Token == request.RefreshToken && !t.IsRevoked && t.ExpiryDate > DateTime.UtcNow,
                include: q => q.Include(t => t.User).ThenInclude(u => u.UserRoles).ThenInclude(ur => ur.Role));
            if (refreshToken == null)
            {
                var errors = new[] { new ErrorDetail("INVALID_TOKEN", "Invalid or expired refresh token.") };
                return ResponseFactory<LoginResponseDto>.BadRequest("Invalid token.", errors);
            }

            // 2. Vô hiệu hóa refresh token cũ
            refreshToken.IsRevoked = true;
            _unitOfWork.Repository.Update(refreshToken);

            // 3. Tạo refresh token mới
            var newRefreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = refreshToken.UserId,
                Token = _jwtService.GenerateRefreshToken(),
                ExpiryDate = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpiryDays"])),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };
            _unitOfWork.Repository.Add(newRefreshToken);

            // 4. Tạo access token mới
            var accessToken = await  _jwtService.GenerateJwtTokenAsync(refreshToken.User);

            await _unitOfWork.SaveChangesAsync(cancellationToken);
            await _unitOfWork.CommitAsync(transaction, cancellationToken);

            // 5. Tạo response
            var responseDto = new LoginResponseDto
            {
                Token = accessToken.Data,
                RefreshToken = newRefreshToken.Token,
                Expiry = newRefreshToken.ExpiryDate,
                //User = new UserDto
                //{
                //    Id = refreshToken.User.Id,
                //    Username = refreshToken.User.Username,
                //    Email = refreshToken.User.Email,
                //    Roles = refreshToken.User.UserRoles.Select(ur => new RoleDto
                //    {
                //        Id = ur.Role.Id,
                //        Name = ur.Role.Name,
                //        Administrator = ur.Role.Administrator
                //    }).ToList()
                //}
            };

            return ResponseFactory<LoginResponseDto>.Ok(responseDto, "Token refreshed successfully.");
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackAsync(transaction, cancellationToken);
            var errors = new[] { new ErrorDetail("INTERNAL_ERROR", $"An unexpected error occurred: {ex.Message}") };
            return ResponseFactory<LoginResponseDto>.InternalServerError("An unexpected error occurred.", errors);
        }
    }
}