using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.DataAccess.Containers.Interfaces;
using MediatR;
using RestaurantService.Application.Dtos;
using RestaurantService.Application.Queries;
using RestaurantService.Domain.Entities;
using System.Linq.Expressions;

namespace RestaurantService.Application.Handlers;

public class FindRestaurantsNearbyHandler : IRequestHandler<FindRestaurantsNearbyQuery, ApiResponseWithData<IEnumerable<RestaurantDto>>>
{
    private readonly IGeoDistanceCalculator _geoCalculator;
    private readonly IEFDALContainer _dalContainer;
    private readonly ILoggerAdapter<FindRestaurantsNearbyHandler> _logger;

    public FindRestaurantsNearbyHandler(
        IGeoDistanceCalculator geoCalculator,
        IEFDALContainer dalContainer,
        ILoggerAdapter<FindRestaurantsNearbyHandler> logger)
    {
        _geoCalculator = geoCalculator ?? throw new ArgumentNullException(nameof(geoCalculator));
        _dalContainer = dalContainer ?? throw new ArgumentNullException(nameof(dalContainer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponseWithData<IEnumerable<RestaurantDto>>> Handle(FindRestaurantsNearbyQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finding restaurants near ({Latitude}, {Longitude}) within {Radius} {Unit}",
            request.Latitude, request.Longitude, request.Radius, request.Unit);

        try
        {
            // Validate coordinates
            if (double.IsNaN(request.Latitude) || double.IsNaN(request.Longitude) ||
                request.Latitude < -90 || request.Latitude > 90 ||
                request.Longitude < -180 || request.Longitude > 180)
            {
                _logger.LogWarning("Invalid coordinates provided: ({Latitude}, {Longitude})", request.Latitude, request.Longitude);
                return ResponseFactory<IEnumerable<RestaurantDto>>.BadRequest(
                    "Invalid coordinates",
                    new[] { new ErrorDetail("INVALID_COORDINATES", "Latitude or Longitude is invalid") });
            }

            // Find restaurants within radius using PostGIS or MongoDB
            var results = await _geoCalculator.FindWithinRadiusBatchAsync(
                request.Latitude,
                request.Longitude,
                request.Radius,
                new List<(double lat, double lon)>(), // Không cần points vì backend (PostGIS/Mongo) tự xử lý
                request.Unit,
                cancellationToken);

            if (!results.Any())
            {
                _logger.LogInformation("No restaurants found near ({Latitude}, {Longitude})", request.Latitude, request.Longitude);
                return ResponseFactory<IEnumerable<RestaurantDto>>.Created(Enumerable.Empty<RestaurantDto>(), "No restaurants found");
            }

            // Create a dictionary for faster distance lookup
            var distanceDict = results.ToDictionary(r => r.Id, r => r.Distance);

            // Fetch restaurant details using EFDALContainer
            var restaurantIds = results.Select(r => r.Id).ToList();
            Expression<Func<Restaurant, bool>> filter = r => restaurantIds.Contains(r.Id);
            var restaurants = await _dalContainer.UnitOfWork.Repository
                .FindAsync(filter, cancellationToken);

            // Map to RestaurantDto
            var restaurantDtos = restaurants.Select(r => new RestaurantDto
            {
                Id = r.Id,
                Name = r.Name,
                Address = r.Address,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                Distance = distanceDict.TryGetValue(r.Id, out var distance) ? distance : 0
            }).ToList();

            _logger.LogInformation("Found {Count} restaurants near ({Latitude}, {Longitude})", restaurantDtos.Count, request.Latitude, request.Longitude);
            return ResponseFactory<IEnumerable<RestaurantDto>>.Created(restaurantDtos, "Restaurants found");
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Operation canceled while finding restaurants near ({Latitude}, {Longitude})", request.Latitude, request.Longitude);
            return ResponseFactory<IEnumerable<RestaurantDto>>.InternalServerError(
                "Operation canceled",
                new[] { new ErrorDetail("OPERATION_CANCELED", "The request was canceled") });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find restaurants near ({Latitude}, {Longitude})", request.Latitude, request.Longitude);
            return ResponseFactory<IEnumerable<RestaurantDto>>.InternalServerError(
                "Failed to find restaurants",
                new[] { new ErrorDetail("INTERNAL_ERROR", ex.Message) });
        }
    }
}


//using FoodDeliverySystem.Common.ApiResponse.Factories;
//using FoodDeliverySystem.Common.ApiResponse.Models;
//using FoodDeliverySystem.Common.Geo.Interfaces;
//using FoodDeliverySystem.Common.Geo.Models;
//using FoodDeliverySystem.Common.Logging;
//using FoodDeliverySystem.DataAccess.Containers.Interfaces;
//using MediatR;
//using RestaurantService.Application.Dtos;
//using RestaurantService.Application.Queries;
//using RestaurantService.Domain.Entities;
//using System.Linq.Expressions;

//namespace RestaurantService.Application.Handlers;

//public class FindRestaurantsNearbyHandler : IRequestHandler<FindRestaurantsNearbyQuery, ApiResponseWithData<IEnumerable<RestaurantDto>>>
//{
//    private readonly IGeoDistanceCalculator _geoCalculator;
//    private readonly IEFDALContainer _dalContainer;
//    private readonly ILoggerAdapter<FindRestaurantsNearbyHandler> _logger;

//    public FindRestaurantsNearbyHandler(
//        IGeoDistanceCalculator geoCalculator,
//        IEFDALContainer dalContainer,
//        ILoggerAdapter<FindRestaurantsNearbyHandler> logger)
//    {
//        _geoCalculator = geoCalculator ?? throw new ArgumentNullException(nameof(geoCalculator));
//        _dalContainer = dalContainer ?? throw new ArgumentNullException(nameof(dalContainer));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//    }

//    public async Task<ApiResponseWithData<IEnumerable<RestaurantDto>>> Handle(FindRestaurantsNearbyQuery request, CancellationToken cancellationToken)
//    {
//        _logger.LogInformation("Finding restaurants near ({Latitude}, {Longitude}) within {Radius} {Unit}",
//            request.Latitude, request.Longitude, request.Radius, request.Unit);

//        try
//        {
//            // Find restaurants within radius using PostGIS
//            var results = await _geoCalculator.FindWithinRadiusBatchAsync(
//                request.Latitude,
//                request.Longitude,
//                request.Radius,
//                request.Unit,
//                cancellationToken);

//            if (!results.Any())
//            {
//                _logger.LogInformation("No restaurants found near ({Latitude}, {Longitude})", request.Latitude, request.Longitude);
//                return ResponseFactory<IEnumerable<RestaurantDto>>.Created(Enumerable.Empty<RestaurantDto>(), "No restaurants found");
//            }

//            // Fetch restaurant details using EFDALContainer
//            var restaurantIds = results.Select(r => r.Id).ToList();
//            Expression<Func<Restaurant, bool>> filter = r => restaurantIds.Contains(r.Id);
//            var restaurants = await _dalContainer.UnitOfWork.Repository
//                .FindAsync(filter, cancellationToken);

//            // Map to RestaurantDto manually
//            var restaurantDtos = restaurants.Select(r => new RestaurantDto
//            {
//                Id = r.Id,
//                Name = r.Name,
//                Address = r.Location.Address,
//                Latitude = r.Location.Latitude,
//                Longitude = r.Location.Longitude,
//                Distance = results.FirstOrDefault(x => x.Id == r.Id)?.Distance ?? 0
//            }).ToList();

//            _logger.LogInformation("Found {Count} restaurants near ({Latitude}, {Longitude})", restaurantDtos.Count, request.Latitude, request.Longitude);
//            return ResponseFactory<IEnumerable<RestaurantDto>>.Created(restaurantDtos, "Restaurants found");
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to find restaurants near ({Latitude}, {Longitude})", request.Latitude, request.Longitude);
//            return ResponseFactory<IEnumerable<RestaurantDto>>.InternalServerError("Failed to find restaurants", new[] { new ErrorDetail("INTERNAL_ERROR", ex.Message) });
//        }
//    }
//}