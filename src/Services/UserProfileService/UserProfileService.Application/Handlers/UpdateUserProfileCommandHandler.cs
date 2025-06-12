using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.DataAccess.Containers.Interfaces;
using MediatR;
using UserProfileService.Application.Commands;
using UserProfileService.Application.Dtos;
using UserProfileService.Domain.Entities;

namespace UserProfileService.Application.Handlers;

public class UpdateUserProfileCommandHandler : IRequestHandler<UpdateUserProfileCommand, ApiResponseWithData<UserProfileDto>>
{
    private readonly IEFDALContainer _dalContainer;
    private readonly ILoggerAdapter<UpdateUserProfileCommandHandler> _logger;

    public UpdateUserProfileCommandHandler(
        IEFDALContainer dalContainer,
        ILoggerAdapter<UpdateUserProfileCommandHandler> logger)
    {
        _dalContainer = dalContainer;
        _logger = logger;
    }

    public async Task<ApiResponseWithData<UserProfileDto>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Updating user profile for UserId {UserId}", request.UserId);
        try
        {
            using var transaction = await _dalContainer.UnitOfWork.BeginTransactionAsync(cancellationToken);

            var userProfile = await _dalContainer.UnitOfWork.Repository
                .FirstOrDefaultAsync<UserProfile>(u => u.UserId == request.UserId, cancellationToken: cancellationToken);
            if (userProfile == null)
            {
                return ResponseFactory<UserProfileDto>.NotFound("User profile not found", new[] { new ErrorDetail("NOT_FOUND", "User profile does not exist.") });
            }

            userProfile.Name = request.Name;
            userProfile.Email = request.Email;
            userProfile.ModifiedAt = DateTime.UtcNow;

            await _dalContainer.UnitOfWork.Repository.UpdateAsync(userProfile, cancellationToken);
            await _dalContainer.UnitOfWork.SaveChangesAsync(cancellationToken);
            await _dalContainer.UnitOfWork.CommitAsync(transaction, cancellationToken);

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

            _logger.LogInformation("UserProfile {UserProfileId} updated", userProfile.Id);
            return ResponseFactory<UserProfileDto>.Ok(result, "User profile updated successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update user profile for UserId {UserId}", request.UserId);
            return ResponseFactory<UserProfileDto>.InternalServerError("Failed to update user profile", new[] { new ErrorDetail("INTERNAL_ERROR", ex.Message) });
        }
    }
}