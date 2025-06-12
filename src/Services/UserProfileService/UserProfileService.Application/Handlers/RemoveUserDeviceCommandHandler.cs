using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.DataAccess.Containers.Interfaces;
using MediatR;
using UserProfileService.Application.Commands;
using UserProfileService.Domain.Entities;

namespace UserProfileService.Application.Handlers;

public class RemoveUserDeviceCommandHandler : IRequestHandler<RemoveUserDeviceCommand, ApiResponseWithData<bool>>
{
    private readonly IEFDALContainer _dalContainer;
    private readonly ILoggerAdapter<RemoveUserDeviceCommandHandler> _logger;

    public RemoveUserDeviceCommandHandler(
        IEFDALContainer dalContainer,
        ILoggerAdapter<RemoveUserDeviceCommandHandler> logger)
    {
        _dalContainer = dalContainer;
        _logger = logger;
    }

    public async Task<ApiResponseWithData<bool>> Handle(RemoveUserDeviceCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing device {DeviceId} for UserId {UserId}", request.DeviceId, request.UserId);
        try
        {
            using var transaction = await _dalContainer.UnitOfWork.BeginTransactionAsync(cancellationToken);

            var device = await _dalContainer.UnitOfWork.Repository
                .FirstOrDefaultAsync<UserDevice>(d => d.Id == request.DeviceId && d.UserId == request.UserId, cancellationToken: cancellationToken);
            if (device == null)
            {
                return ResponseFactory<bool>.NotFound("Device not found", new[] { new ErrorDetail("NOT_FOUND", "Device does not exist.") });
            }

            await _dalContainer.UnitOfWork.Repository.DeleteAsync<UserDevice>(d => d.Id == request.DeviceId, cancellationToken);
            await _dalContainer.UnitOfWork.SaveChangesAsync(cancellationToken);
            await _dalContainer.UnitOfWork.CommitAsync(transaction, cancellationToken);

            _logger.LogInformation("Device {DeviceId} removed for UserId {UserId}", request.DeviceId, request.UserId);
            return ResponseFactory<bool>.Ok(true, "Device removed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove device {DeviceId} for UserId {UserId}", request.DeviceId, request.UserId);
            return ResponseFactory<bool>.InternalServerError("Failed to remove device", new[] { new ErrorDetail("INTERNAL_ERROR", ex.Message) });
        }
    }
}