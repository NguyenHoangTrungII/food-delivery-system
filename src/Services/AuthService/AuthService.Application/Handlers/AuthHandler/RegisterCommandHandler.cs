using AuthService.Application.Commands.Auth;
using AuthService.Application.Dtos;
using AuthService.Application.Services;
using AuthService.Domain.Entities;
using AuthService.Infrastructure.IRepositories;
using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.Common.ServiceClient.Interfaces;
using FoodDeliverySystem.Common.ServiceClient.Models;
using MediatR;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;

namespace AuthService.Application.CommandHandlers.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponseWithData<RegisterResponseDto>>
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtService _jwtService;
    private readonly IServiceClient _serviceClient;
    private readonly IConfiguration _configuration;
    private readonly ILoggerAdapter<RegisterCommandHandler> _logger;

    public RegisterCommandHandler(
        IUnitOfWork unitOfWork,
        IJwtService jwtService,
        IServiceClient serviceClient,
        IConfiguration configuration,
        ILoggerAdapter<RegisterCommandHandler> logger)
    {
        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
        _jwtService = jwtService ?? throw new ArgumentNullException(nameof(jwtService));
        _serviceClient = serviceClient ?? throw new ArgumentNullException(nameof(serviceClient));
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponseWithData<RegisterResponseDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
        try
        {
            _logger.LogInformation("Starting registration for user with email {Email}", request.Email);

            // 1. Kiểm tra xem user đã tồn tại chưa
            var existingUser = await _unitOfWork.Repository.FirstOrDefaultAsync<User>(
                u => u.Email == request.Email || u.Username == request.Username,
                cancellationToken: cancellationToken);
            if (existingUser != null)
            {
                _logger.LogWarning("User with email {Email} or username {Username} already exists.", request.Email, request.Username);
                return ResponseFactory<RegisterResponseDto>.BadRequest("User already exists.", new[] {
                    new ErrorDetail("USER_EXISTS", "A user with the same email or username already exists.")
                });
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
            //await _unitOfWork.Repository.AddAsync(user, cancellationToken);
            _unitOfWork.Repository.Add(user);


            // 3. Tạo refresh token
            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),
                UserId = user.Id,
                Token = _jwtService.GenerateRefreshToken(),
                ExpiryDate = DateTime.UtcNow.AddDays(double.Parse(_configuration["Jwt:RefreshTokenExpiryDays"])),
                IsRevoked = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = "system"
            };
            //await _unitOfWork.Repository.AddAsync(refreshToken, cancellationToken);
            _unitOfWork.Repository.Add(refreshToken);

            // 4. Lưu thay đổi cục bộ
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // 5. Gọi API UserProfileService để tạo profile
            var profileRequest = new
            {
                UserId = user.Id,
                Name = request.Username,
                Email = request.Email
            };

            var profileResponse = await _serviceClient.PostAsync<ApiResponseWithData<Guid>>(
                "/api/UserProfiles",
                profileRequest,
                cancellationToken: cancellationToken);

            if (profileResponse.Status != "success")
            {
                _logger.LogInformation("Failed to create user profile for UserId {UserId}. Errors: {Errors}", user.Id, string.Join("; ", profileResponse.Errors.Select(e => e.Message)));
                await _unitOfWork.RollbackAsync(transaction, cancellationToken);
                return ResponseFactory<RegisterResponseDto>.InternalServerError("Failed to create user profile.", profileResponse.Errors);
            }

            // 6. Tạo access token
            var accessToken = await _jwtService.GenerateJwtTokenAsync(user);

            // 7. Commit giao dịch
            await _unitOfWork.CommitAsync(transaction, cancellationToken);

            _logger.LogInformation("User {UserId} registered successfully with profile.", user.Id);

            // 8. Tạo response
            var responseDto = new RegisterResponseDto
            {
                UserId = user.Id,
                Username = user.Username,
                Email = user.Email,
                Token = accessToken.Data,
                RefreshToken = refreshToken.Token,
                Expiry = refreshToken.ExpiryDate
            };

            return ResponseFactory<RegisterResponseDto>.Created(responseDto, "User registered successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to register user with email {Email}", request.Email);
            await _unitOfWork.RollbackAsync(transaction, cancellationToken);
            return ResponseFactory<RegisterResponseDto>.InternalServerError("Failed to register user.", new[] {
                new ErrorDetail("SERVER_ERROR", ex.Message)
            });
        }
    }
}

//using AuthService.Application.Commands.Auth;
//using AuthService.Application.Dtos;
//using AuthService.Domain.Entities;
//using AuthService.Infrastructure.IRepositories;
//using MediatR;
//using Microsoft.EntityFrameworkCore.Storage;
//using BCrypt.Net;
//using FoodDeliverySystem.Common.ApiResponse.Factories;
//using FoodDeliverySystem.Common.ApiResponse.Models;

//namespace AuthService.Application.CommandHandlers.Auth;

//public class RegisterCommandHandler : IRequestHandler<RegisterCommand, ApiResponseWithData<UserDto>>
//{
//    private readonly IUnitOfWork _unitOfWork;
//    private const string DefaultCustomerRoleName = "Customer";

//    public RegisterCommandHandler(IUnitOfWork unitOfWork)
//    {
//        _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
//    }

//    public async Task<ApiResponseWithData<UserDto>> Handle(RegisterCommand request, CancellationToken cancellationToken)
//    {
//        using var transaction = await _unitOfWork.BeginTransactionAsync(cancellationToken);
//        try
//        {
//            // 1. Kiểm tra username hoặc email đã tồn tại
//            var existingUser = await _unitOfWork.Repository.FirstOrDefaultAsync<User>(
//                u => u.Username == request.Username || u.Email == request.Email);
//            if (existingUser != null)
//            {
//                var errors = new[] { new ErrorDetail("CONFLICT", "Username or email already exists.") };
//                return ResponseFactory<UserDto>.BadRequest("Registration failed.", errors);
//            }

//            // 2. Tạo user mới
//            var user = new User
//            {
//                Id = Guid.NewGuid(),
//                Username = request.Username,
//                Email = request.Email,
//                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
//                CreatedAt = DateTime.UtcNow,
//                CreatedBy = "system"
//            };

//            var customerRole = await _unitOfWork.Repository.FirstOrDefaultAsync<Role>(
//                r => r.Name == DefaultCustomerRoleName);
//            if (customerRole == null)
//            {
//                customerRole = new Role
//                {
//                    Id = Guid.NewGuid(),
//                    Name = DefaultCustomerRoleName,
//                    Administrator = false,
//                    CreatedOn = DateTime.UtcNow,
//                    CreatedBy = "system"
//                };
//                _unitOfWork.Repository.Add(customerRole);
//            }

//            // 4. Gán role "Customer" cho user
//            var userRole = new UserRole
//            {
//                Id = Guid.NewGuid(),
//                UserId = user.Id,
//                RoleId = customerRole.Id,
//                StartDate = DateTime.UtcNow,
//                CreatedOn = DateTime.UtcNow,
//                CreatedBy = "system"
//            };
//            user.UserRoles.Add(userRole);

//            _unitOfWork.Repository.Add(user);
//            await _unitOfWork.SaveChangesAsync(cancellationToken);

//            await _unitOfWork.CommitAsync(transaction, cancellationToken);

//            // 3. Tạo response
//            var responseDto = new UserDto
//            {
//                Id = user.Id,
//                Username = user.Username,
//                Email = user.Email,
//                Roles = new List<RoleDto>()
//            };


//            return ResponseFactory<UserDto>.Created(responseDto, "User registered successfully.");
//        }
//        catch (Exception ex)
//        {
//            await _unitOfWork.RollbackAsync(transaction, cancellationToken);
//            var errors = new[] { new ErrorDetail("INTERNAL_ERROR", $"An unexpected error occurred: {ex.Message}") };
//            return ResponseFactory<UserDto>.InternalServerError("An unexpected error occurred.", errors);
//        }
//    }
//}