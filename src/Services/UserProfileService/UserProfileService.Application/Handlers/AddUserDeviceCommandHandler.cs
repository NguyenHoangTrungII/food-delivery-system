using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.DataAccess.Containers.Interfaces;
using MediatR;
using UserProfileService.Application.Commands;
using UserProfileService.Application.Dtos;
using UserProfileService.Domain.Entities;

namespace UserProfileService.Application.Handlers;

public class AddUserDeviceCommandHandler : IRequestHandler<AddUserDeviceCommand, ApiResponseWithData<UserDeviceDto>>
{
    private readonly IEFDALContainer _dalContainer;
    private readonly ILoggerAdapter<AddUserDeviceCommandHandler> _logger;

    public AddUserDeviceCommandHandler(
        IEFDALContainer dalContainer,
        ILoggerAdapter<AddUserDeviceCommandHandler> logger)
    {
        _dalContainer = dalContainer;
        _logger = logger;
    }

    public async Task<ApiResponseWithData<UserDeviceDto>> Handle(AddUserDeviceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding device for UserId {UserId}", request.UserId);
        try
        {
            using var transaction = await _dalContainer.UnitOfWork.BeginTransactionAsync(cancellationToken);

            var userProfile = await _dalContainer.UnitOfWork.Repository
                .FirstOrDefaultAsync<UserProfile>(u => u.UserId == request.UserId, cancellationToken: cancellationToken);
            if (userProfile == null)
            {
                return ResponseFactory<UserDeviceDto>.NotFound("User profile not found", new[] { new ErrorDetail("NOT_FOUND", "User profile does not exist.") });
            }

            var deviceExists = await _dalContainer.UnitOfWork.Repository
                .FirstOrDefaultAsync<UserDevice>(d => d.UserId == request.UserId && d.DeviceId == request.DeviceId, cancellationToken: cancellationToken);
            if (deviceExists != null)
            {
                return ResponseFactory<UserDeviceDto>.BadRequest("Device already exists", new[] { new ErrorDetail("CONFLICT", "DeviceId already registered.") });
            }

            var device = new UserDevice
            {
                UserId = request.UserId,
                DeviceId = request.DeviceId,
                DeviceName = request.DeviceName,
                OperatingSystem = request.OperatingSystem,
                LastLogin = request.LastLogin
            };

            await _dalContainer.UnitOfWork.Repository.AddAsync(device, cancellationToken);
            await _dalContainer.UnitOfWork.SaveChangesAsync(cancellationToken);
            await _dalContainer.UnitOfWork.CommitAsync(transaction, cancellationToken);

            var result = new UserDeviceDto
            {
                Id = device.Id,
                UserId = device.UserId,
                DeviceId = device.DeviceId,
                DeviceName = device.DeviceName,
                OperatingSystem = device.OperatingSystem,
                LastLogin = device.LastLogin
            };

            _logger.LogInformation("Device {DeviceId} added for UserId {UserId}", device.Id, request.UserId);
            return ResponseFactory<UserDeviceDto>.Created(result, "Device added successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add device for UserId {UserId}", request.UserId);
            return ResponseFactory<UserDeviceDto>.InternalServerError("Failed to add device", new[] { new ErrorDetail("INTERNAL_ERROR", ex.Message) });
        }
    }
}