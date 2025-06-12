using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.DataAccess.Containers.Interfaces;
using MediatR;
using UserProfileService.Application.Commands;
using UserProfileService.Domain.Entities;

namespace UserProfileService.Application.Handlers;

public class RemoveAddressCommandHandler : IRequestHandler<RemoveAddressCommand, ApiResponseWithData<bool>>
{
    private readonly IEFDALContainer _dalContainer;
    private readonly ILoggerAdapter<RemoveAddressCommandHandler> _logger;

    public RemoveAddressCommandHandler(
        IEFDALContainer dalContainer,
        ILoggerAdapter<RemoveAddressCommandHandler> logger)
    {
        _dalContainer = dalContainer;
        _logger = logger;
    }

    public async Task<ApiResponseWithData<bool>> Handle(RemoveAddressCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Removing address {AddressId} for UserId {UserId}", request.AddressId, request.UserId);
        try
        {
            using var transaction = await _dalContainer.UnitOfWork.BeginTransactionAsync(cancellationToken);

            var address = await _dalContainer.UnitOfWork.Repository
                .FirstOrDefaultAsync<Address>(a => a.Id == request.AddressId && a.UserId == request.UserId, cancellationToken: cancellationToken);
            if (address == null)
            {
                return ResponseFactory<bool>.NotFound("Address not found", new[] { new ErrorDetail("NOT_FOUND", "Address does not exist.") });
            }

            await _dalContainer.UnitOfWork.Repository.DeleteAsync<Address>(a => a.Id == request.AddressId, cancellationToken);
            await _dalContainer.UnitOfWork.SaveChangesAsync(cancellationToken);
            await _dalContainer.UnitOfWork.CommitAsync(transaction, cancellationToken);

            _logger.LogInformation("Address {AddressId} removed for UserId {UserId}", request.AddressId, request.UserId);
            return ResponseFactory<bool>.Ok(true, "Address removed successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove address {AddressId} for UserId {UserId}", request.AddressId, request.UserId);
            return ResponseFactory<bool>.InternalServerError("Failed to remove address", new[] { new ErrorDetail("INTERNAL_ERROR", ex.Message) });
        }
    }
}