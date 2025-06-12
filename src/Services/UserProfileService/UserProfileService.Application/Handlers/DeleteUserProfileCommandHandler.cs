using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.DataAccess.Containers.Interfaces;
using MediatR;
using UserProfileService.Application.Commands;
using UserProfileService.Domain.Entities;

namespace UserProfileService.Application.Handlers;

public class DeleteUserProfileCommandHandler : IRequestHandler<DeleteUserProfileCommand, ApiResponseWithData<bool>>
{
    private readonly IEFDALContainer _dalContainer;
    private readonly ILoggerAdapter<DeleteUserProfileCommandHandler> _logger;

    public DeleteUserProfileCommandHandler(
        IEFDALContainer dalContainer,
        ILoggerAdapter<DeleteUserProfileCommandHandler> logger)
    {
        _dalContainer = dalContainer;
        _logger = logger;
    }

    public async Task<ApiResponseWithData<bool>> Handle(DeleteUserProfileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Deleting user profile for UserId {UserId}", request.UserId);
        try
        {
            using var transaction = await _dalContainer.UnitOfWork.BeginTransactionAsync(cancellationToken);

            var userProfileExists = await _dalContainer.UnitOfWork.Repository
                .FirstOrDefaultAsync<UserProfile>(u => u.UserId == request.UserId, cancellationToken: cancellationToken);
            if (userProfileExists == null)
            {
                return ResponseFactory<bool>.NotFound("User profile not found", new[] { new ErrorDetail("NOT_FOUND", "User profile does not exist.") });
            }

            await _dalContainer.UnitOfWork.Repository.DeleteAsync<UserProfile>(u => u.UserId == request.UserId, cancellationToken);
            await _dalContainer.UnitOfWork.SaveChangesAsync(cancellationToken);
            await _dalContainer.UnitOfWork.CommitAsync(transaction, cancellationToken);

            _logger.LogInformation("UserProfile for UserId {UserId} deleted", request.UserId);
            return ResponseFactory<bool>.Ok(true, "User profile deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete user profile for UserId {UserId}", request.UserId);
            return ResponseFactory<bool>.InternalServerError("Failed to delete user profile", new[] { new ErrorDetail("INTERNAL_ERROR", ex.Message) });
        }
    }
}