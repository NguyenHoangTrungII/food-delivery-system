using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.DataAccess.Containers.Interfaces;
using MediatR;
using UserProfileService.Application.Commands;
using UserProfileService.Application.Dtos;
using UserProfileService.Domain.Entities;

namespace UserProfileService.Application.Handlers;

public class AddAddressCommandHandler : IRequestHandler<AddAddressCommand, ApiResponseWithData<AddressDto>>
{
    private readonly IEFDALContainer _dalContainer;
    private readonly ILoggerAdapter<AddAddressCommandHandler> _logger;

    public AddAddressCommandHandler(
        IEFDALContainer dalContainer,
        ILoggerAdapter<AddAddressCommandHandler> logger)
    {
        _dalContainer = dalContainer;
        _logger = logger;
    }

    public async Task<ApiResponseWithData<AddressDto>> Handle(AddAddressCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Adding address for UserId {UserId}", request.UserId);
        try
        {
            using var transaction = await _dalContainer.UnitOfWork.BeginTransactionAsync(cancellationToken);

            var userProfile = await _dalContainer.UnitOfWork.Repository
                .FirstOrDefaultAsync<UserProfile>(u => u.UserId == request.UserId, cancellationToken: cancellationToken);
            if (userProfile == null)
            {
                return ResponseFactory<AddressDto>.NotFound("User profile not found", new[] { new ErrorDetail("NOT_FOUND", "User profile does not exist.") });
            }

            var address = new Address
            {
                UserId = request.UserId,
                Street = request.Street,
                City = request.City,
                PostalCode = request.PostalCode,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                IsDefault = request.IsDefault
            };

            await _dalContainer.UnitOfWork.Repository.AddAsync(address, cancellationToken);
            await _dalContainer.UnitOfWork.SaveChangesAsync(cancellationToken);
            await _dalContainer.UnitOfWork.CommitAsync(transaction, cancellationToken);

            var result = new AddressDto
            {
                Id = address.Id,
                UserId = address.UserId,
                Street = address.Street,
                City = address.City,
                PostalCode = address.PostalCode,
                Latitude = address.Latitude,
                Longitude = address.Longitude,
                IsDefault = address.IsDefault
            };

            _logger.LogInformation("Address {AddressId} added for UserId {UserId}", address.Id, request.UserId);
            return ResponseFactory<AddressDto>.Created(result, "Address added successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to add address for UserId {UserId}", request.UserId);
            return ResponseFactory<AddressDto>.InternalServerError("Failed to add address", new[] { new ErrorDetail("INTERNAL_ERROR", ex.Message) });
        }
    }
}