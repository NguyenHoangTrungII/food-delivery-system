using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.DataAccess.Containers.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using UserProfileService.Application.Dtos;
using UserProfileService.Application.Queries;
using UserProfileService.Domain.Entities;

namespace UserProfileService.Application.Handlers;

public class GetUserProfileByIdQueryHandler : IRequestHandler<GetUserProfileByIdQuery, ApiResponseWithData<UserProfileDto>>
{
    private readonly IEFDALContainer _dalContainer;
    private readonly ILoggerAdapter<GetUserProfileByIdQueryHandler> _logger;

    public GetUserProfileByIdQueryHandler(
        IEFDALContainer dalContainer,
        ILoggerAdapter<GetUserProfileByIdQueryHandler> logger)
    {
        _dalContainer = dalContainer;
        _logger = logger;
    }

    public async Task<ApiResponseWithData<UserProfileDto>> Handle(GetUserProfileByIdQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Fetching user profile for UserId {UserId}", request.UserId);
        try
        {
            var userProfile = await _dalContainer.UnitOfWork.Repository
                .FirstOrDefaultAsync<UserProfile>(
                    u => u.UserId == request.UserId,
                    q => q.Include(u => u.Addresses).Include(u => u.Devices),
                    cancellationToken);

            if (userProfile == null)
            {
                return ResponseFactory<UserProfileDto>.NotFound("User profile not found", new[] { new ErrorDetail("NOT_FOUND", "User profile does not exist.") });
            }

            var result = new UserProfileDto
            {
                Id = userProfile.Id,
                UserId = userProfile.UserId,
                Name = userProfile.Name,
                Email = userProfile.Email,
                Addresses = userProfile.Addresses.Select(a => new AddressDto
                {
                    Id = a.Id,
                    UserId = a.UserId,
                    Street = a.Street,
                    City = a.City,
                    PostalCode = a.PostalCode,
                    Latitude = a.Latitude,
                    Longitude = a.Longitude,
                    IsDefault = a.IsDefault
                }).ToList(),
                Devices = userProfile.Devices.Select(d => new UserDeviceDto
                {
                    Id = d.Id,
                    UserId = d.UserId,
                    DeviceId = d.DeviceId,
                    DeviceName = d.DeviceName,
                    OperatingSystem = d.OperatingSystem,
                    LastLogin = d.LastLogin
                }).ToList()
            };

            _logger.LogInformation("UserProfile {UserProfileId} retrieved", userProfile.Id);
            return ResponseFactory<UserProfileDto>.Ok(result, "User profile retrieved successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve user profile for UserId {UserId}", request.UserId);
            return ResponseFactory<UserProfileDto>.InternalServerError("Failed to retrieve user profile", new[] { new ErrorDetail("INTERNAL_ERROR", ex.Message) });
        }
    }
}