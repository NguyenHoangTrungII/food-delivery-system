using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.DataAccess.Containers.Interfaces;
using MediatR;
using UserProfileService.Application.Commands;
using UserProfileService.Domain.Entities;

public class CreateUserProfileCommandHandler : IRequestHandler<CreateUserProfileCommand, ApiResponseWithData<Guid>>
{
    private readonly IEFDALContainer _dalContainer;
    private readonly ILoggerAdapter<CreateUserProfileCommandHandler> _logger;

    public CreateUserProfileCommandHandler(
        IEFDALContainer dalContainer,
        ILoggerAdapter<CreateUserProfileCommandHandler> logger)
    {
        _dalContainer = dalContainer;
        _logger = logger;
    }

    public async Task<ApiResponseWithData<Guid>> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Creating user profile for UserId {UserId}", request.UserId);

        try
        {
            var userProfile = new UserProfile
            {
                UserId = request.UserId,
                Name = request.Name,
                Email = request.Email
            };

            await _dalContainer.UnitOfWork.Repository.AddAsync(userProfile, cancellationToken);
            await _dalContainer.UnitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("UserProfile {UserProfileId} created", userProfile.Id);
            return ResponseFactory<Guid>.Created(userProfile.Id, "User profile created successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create user profile for UserId {UserId}", request.UserId);
            return ResponseFactory<Guid>.InternalServerError("Failed to create user profile", new[] {
                new ErrorDetail("SERVER_ERROR", ex.Message)
            });
        }
    }
}

//using FoodDeliverySystem.Common.ApiResponse.Factories;
//using FoodDeliverySystem.Common.ApiResponse.Models;
//using FoodDeliverySystem.Common.Logging;
//using FoodDeliverySystem.DataAccess.Containers.Interfaces;
//using MediatR;
//using UserProfileService.Application.Commands;
//using UserProfileService.Domain.Entities;
//using Microsoft.EntityFrameworkCore;

//namespace UserProfileService.Application.Handlers;

//public class CreateUserProfileCommandHandler : IRequestHandler<CreateUserProfileCommand, ApiResponseWithData<Guid>>
//{
//    private readonly IEFDALContainer _dalContainer;
//    private readonly ILoggerAdapter<CreateUserProfileCommandHandler> _logger;

//    public CreateUserProfileCommandHandler(
//        IEFDALContainer dalContainer,
//        ILoggerAdapter<CreateUserProfileCommandHandler> logger)
//    {
//        _dalContainer = dalContainer;
//        _logger = logger;
//    }

//    public async Task<ApiResponseWithData<Guid>> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
//    {
//        _logger.LogInformation("Creating user profile for UserId {UserId}", request.UserId);

//        try
//        {
//            // Lấy strategy để retry và bao toàn bộ transaction
//            var strategy = _dalContainer.DbContext.Database.CreateExecutionStrategy();

//            return await strategy.ExecuteAsync(async () =>
//            {
//                await using var transaction = await _dalContainer.UnitOfWork.BeginTransactionAsync(cancellationToken);

//                var userProfile = new UserProfile
//                {
//                    UserId = request.UserId,
//                    Name = request.Name,
//                    Email = request.Email
//                };

//                await _dalContainer.UnitOfWork.Repository.AddAsync(userProfile, cancellationToken);
//                await _dalContainer.UnitOfWork.SaveChangesAsync(cancellationToken);
//                await _dalContainer.UnitOfWork.CommitAsync(transaction, cancellationToken);

//                _logger.LogInformation("UserProfile {UserProfileId} created", userProfile.Id);
//                return ResponseFactory<Guid>.Created(userProfile.Id, "User profile created successfully.");
//            });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to create user profile for UserId {UserId}", request.UserId);
//            return ResponseFactory<Guid>.InternalServerError("Failed to create user profile", new[] {
//                new ErrorDetail("SERVER_ERROR", ex.Message)
//            });
//        }
//    }
//}


////using FoodDeliverySystem.Common.ApiResponse.Factories;
////using FoodDeliverySystem.Common.ApiResponse.Models;
////using FoodDeliverySystem.Common.Logging;
////using FoodDeliverySystem.DataAccess.Containers.Interfaces;
////using MediatR;
////using UserProfileService.Application.Commands;
////using UserProfileService.Domain.Entities;

////namespace UserProfileService.Application.Handlers;

////public class CreateUserProfileCommandHandler : IRequestHandler<CreateUserProfileCommand, ApiResponseWithData<Guid>>
////{
////    private readonly IEFDALContainer _dalContainer;
////    private readonly ILoggerAdapter<CreateUserProfileCommandHandler> _logger;

////    public CreateUserProfileCommandHandler(
////        IEFDALContainer dalContainer,
////        ILoggerAdapter<CreateUserProfileCommandHandler> logger)
////    {
////        _dalContainer = dalContainer;
////        _logger = logger;
////    }

////    public async Task<ApiResponseWithData<Guid>> Handle(CreateUserProfileCommand request, CancellationToken cancellationToken)
////    {
////        _logger.LogInformation("Creating user profile for UserId {UserId}", request.UserId);
////        try
////        {
////            using var transaction = await _dalContainer.UnitOfWork.BeginTransactionAsync(cancellationToken);

////            var userProfile = new UserProfile
////            {
////                UserId = request.UserId,
////                Name = request.Name,
////                Email = request.Email
////            };

////            await _dalContainer.UnitOfWork.Repository.AddAsync(userProfile, cancellationToken);
////            await _dalContainer.UnitOfWork.SaveChangesAsync(cancellationToken);
////            await _dalContainer.UnitOfWork.CommitAsync(transaction, cancellationToken);

////            _logger.LogInformation("UserProfile {UserProfileId} created", userProfile.Id);
////            return ResponseFactory<Guid>.Created(userProfile.Id, "User profile created successfully.");
////        }
////        catch (Exception ex)
////        {
////            _logger.LogError(ex, "Failed to create user profile for UserId {UserId}", request.UserId);
////            return ResponseFactory<Guid>.InternalServerError("Failed to create user profile", new[] { new ErrorDetail("SERVER_ERROR", ex.Message) });
////        }
////    }
////}