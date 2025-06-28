using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Geo.Common;
using FoodDeliverySystem.Common.Geo.Configurations;
using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.Common.Geo.Models;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.DataAccess.Containers.Interfaces;
using MediatR;
using RestaurantService.Application.Dtos;
using RestaurantService.Application.Queries;
using RestaurantService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using static FoodDeliverySystem.Common.Geo.Extensions.ServiceCollectionExtensions;


namespace RestaurantService.Application.Handlers;

public class FindRestaurantsNearbyHandler : IRequestHandler<FindRestaurantsNearbyQuery, ApiResponseWithData<IEnumerable<RestaurantDto>>>
{
    private readonly IGeoDistanceCalculatorFactory _calculatorFactory;
    private readonly IGeoDatabaseProvider _geoDatabaseProvider;
    private readonly IEFDALContainer _dalContainer;
    private readonly IDeliveryFeeCalculator _feeCalculator;
    private readonly IETACalculator _etaCalculator;
    private readonly IGeocodingService _geocodingService;
    private readonly ILoggerAdapter<FindRestaurantsNearbyHandler> _logger;

    private const string PostGisCalculatorKey = "PostGIS";
    private const string OsrmCalculatorKey = "OSRM";

    public FindRestaurantsNearbyHandler(
        IGeoDistanceCalculatorFactory calculatorFactory,
        IGeoDatabaseProvider geoDatabaseProvider,
        IEFDALContainer dalContainer,
        IDeliveryFeeCalculator feeCalculator,
        IETACalculator etaCalculator,
        IGeocodingService geocodingService,
        ILoggerAdapter<FindRestaurantsNearbyHandler> logger)
    {
        _calculatorFactory = calculatorFactory ?? throw new ArgumentNullException(nameof(calculatorFactory));
        _geoDatabaseProvider = geoDatabaseProvider ?? throw new ArgumentNullException(nameof(geoDatabaseProvider));
        _dalContainer = dalContainer ?? throw new ArgumentNullException(nameof(dalContainer));
        _feeCalculator = feeCalculator ?? throw new ArgumentNullException(nameof(feeCalculator));
        _etaCalculator = etaCalculator ?? throw new ArgumentNullException(nameof(etaCalculator));
        _geocodingService = geocodingService ?? throw new ArgumentNullException(nameof(geocodingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task<ApiResponseWithData<IEnumerable<RestaurantDto>>> Handle(FindRestaurantsNearbyQuery request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Finding restaurants near ({Latitude}, {Longitude}) within {Radius} {Unit} using {MapProvider}",
            request.Latitude, request.Longitude, request.Radius, request.Unit, request.MapProvider);

        try
        {
            // NEW: Validate tọa độ
            if (!GeoValidator.IsValidCoordinate(request.Latitude, request.Longitude))
            {
                _logger.LogWarning("Invalid coordinates: ({Latitude}, {Longitude})", request.Latitude, request.Longitude);
                return ResponseFactory<IEnumerable<RestaurantDto>>.BadRequest(
                    "Invalid coordinates",
                    new[] { new ErrorDetail("INVALID_COORDINATES", "Latitude or Longitude is invalid") });
            }

            // CHANGED: Lọc nhà hàng bằng PostGIS với IGeoDatabaseProvider
            var geoResults = await _geoDatabaseProvider.FindWithinRadiusAsync(
                request.Latitude,
                request.Longitude,
                request.Radius * (request.Unit == GeoUnit.Kilometers ? 1000 : 1609.34),
                "restaurants",
                cancellationToken);

            if (!geoResults.Any())
            {
                _logger.LogInformation("No restaurants found near ({Latitude}, {Longitude}) after PostGIS filter",
                    request.Latitude, request.Longitude);
                return ResponseFactory<IEnumerable<RestaurantDto>>.NotFound(
                    "No restaurants found",
                    new[] { new ErrorDetail("NO_RESULTS", "No restaurants within the specified radius") });
            }

            // CHANGED: Tính khoảng cách thực tế bằng OSRM
            var osrmCalculator = _calculatorFactory.GetCalculator(OsrmCalculatorKey);
            var drivingResults = await osrmCalculator.FindWithinRadiusBatchAsync(
                request.Latitude,
                request.Longitude,
                request.Radius,
                geoResults.Select(r => (r.Id, r.Latitude, r.Longitude)).ToList(),
                request.Unit,
                cancellationToken);

            if (!drivingResults.Any())
            {
                _logger.LogInformation("No restaurants found near ({Latitude}, {Longitude}) after OSRM calculation",
                    request.Latitude, request.Longitude);
                return ResponseFactory<IEnumerable<RestaurantDto>>.NotFound(
                    "No restaurants found",
                    new[] { new ErrorDetail("NO_RESULTS", "No restaurants within the specified radius") });
            }

            // CHANGED: Lấy nhà hàng bằng GenericRepository từ EFDALContainer
            var restaurantIds = drivingResults.Select(r => r.Id).ToList();
            var restaurants = await _dalContainer.UnitOfWork.Repository.GetAsync<Restaurant>(
                r => restaurantIds.Contains(r.Id))
                .ToListAsync(cancellationToken);

            if (!restaurants.Any())
            {
                _logger.LogInformation("No restaurants found in repository for IDs: {Ids}", string.Join(", ", restaurantIds));
                return ResponseFactory<IEnumerable<RestaurantDto>>.NotFound(
                    "No restaurants found",
                    new[] { new ErrorDetail("NO_RESULTS", "No restaurants match the provided IDs") });
            }

            // NEW: Tính phí, ETA, và geocoding song song
            var feeTasks = restaurants.Select(r => _feeCalculator.CalculateFeeAsync(
                request.Latitude, request.Longitude, r.Latitude, r.Longitude,
                request.FeeConfig ?? new FeeConfig(), request.Unit));
            var etaTasks = restaurants.Select(r => _etaCalculator.CalculateETAAsync(
                request.Latitude, request.Longitude, r.Latitude, r.Longitude,
                request.ETAConfig ?? new ETAConfig(), request.Unit));
            var geocodeTasks = restaurants.Select(r => _geocodingService.ReverseGeocodeAsync(
                r.Latitude, r.Longitude, cancellationToken));

            var fees = await Task.WhenAll(feeTasks);
            var etas = await Task.WhenAll(etaTasks);
            var geocodedAddresses = await Task.WhenAll(geocodeTasks);

            // CHANGED: Map sang DTO
            var distanceDict = drivingResults.ToDictionary(r => r.Id, r => r.Distance);
            var restaurantDtos = restaurants.Select((r, i) => new RestaurantDto
            {
                Id = r.Id,
                Name = r.Name,
                Address = r.Address,
                Latitude = r.Latitude,
                Longitude = r.Longitude,
                Distance = distanceDict.TryGetValue(r.Id, out var distance) ? distance : null,
                DeliveryFee = fees[i],
                ETA = etas[i],
                GeocodedAddress = geocodedAddresses[i],
                TransportMode = request.TransportMode
            }).ToList();

            //_logger.LogInformation("Found {Count} restaurants near ({Latitude}, {Longitude})", restaurantDtos.Count, request.Latitude, request.Longitude);
            return ResponseFactory<IEnumerable<RestaurantDto>>.Ok(restaurantDtos, "Restaurants found");
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Operation canceled while finding restaurants near ({Latitude}, {Longitude})", request.Latitude, request.Longitude);
            return ResponseFactory<IEnumerable<RestaurantDto>>.BadRequest(
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
//using FoodDeliverySystem.Common.Logging;
//using FoodDeliverySystem.DataAccess.Containers.Interfaces;
//using MediatR;
//using Microsoft.EntityFrameworkCore;
//using RestaurantService.Application.Dtos;
//using RestaurantService.Application.Queries;
//using RestaurantService.Domain.Entities;
//using static FoodDeliverySystem.Common.Geo.Extensions.ServiceCollectionExtensions;

//namespace RestaurantService.Application.Handlers;

//public class FindRestaurantsNearbyHandler : IRequestHandler<FindRestaurantsNearbyQuery, ApiResponseWithData<IEnumerable<RestaurantDto>>>
//{
//    private readonly IGeoDistanceCalculatorFactory _calculatorFactory;
//    private readonly IGeoDistanceCalculator _geoCalculator;
//    private readonly IGeoDatabaseProvider _geoDatabaseProvider;
//    private readonly IEFDALContainer _dalContainer;
//    private readonly ILoggerAdapter<FindRestaurantsNearbyHandler> _logger;

//    private const string PostGisCalculatorKey = "PostGIS";
//    private const string OsrmCalculatorKey = "OSRM";

//    public FindRestaurantsNearbyHandler(
//        IGeoDistanceCalculator geoCalculator,
//        IEFDALContainer dalContainer,
//        ILoggerAdapter<FindRestaurantsNearbyHandler> logger,
//        IGeoDistanceCalculatorFactory calculatorFactory,
//        IGeoDatabaseProvider geoDatabaseProvider = null)
//    {
//        _geoCalculator = geoCalculator ?? throw new ArgumentNullException(nameof(geoCalculator));
//        _dalContainer = dalContainer ?? throw new ArgumentNullException(nameof(dalContainer));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
//        _calculatorFactory = calculatorFactory;
//        _geoDatabaseProvider = geoDatabaseProvider;
//    }

//    public async Task<ApiResponseWithData<IEnumerable<RestaurantDto>>> Handle(FindRestaurantsNearbyQuery request, CancellationToken cancellationToken)
//    {
//        _logger.LogInformation("Finding restaurants near ({Latitude}, {Longitude}) within {Radius} {Unit}",
//            request.Latitude, request.Longitude, request.Radius, request.Unit);

//        try
//        {
//            //10.7765 106.7000 0.3 0
//            //var postgisCalculator = _calculatorFactory.GetCalculator(PostGisCalculatorKey);
//            var osrmCalculator = _calculatorFactory.GetCalculator(OsrmCalculatorKey);

//            var geoResults = await _geoDatabaseProvider.FindWithinRadiusAsync(
//                request.Latitude,
//                request.Longitude,
//                request.Radius,
//                "Restaurants",
//                cancellationToken);

//            // Bước 2: Dùng kết quả PostGIS lọc để truyền vào OSRM
//            if (!geoResults.Any())
//            {
//                _logger.LogInformation("No restaurants found near ({Latitude}, {Longitude}) after PostGIS filter",
//                    request.Latitude, request.Longitude);
//                return ResponseFactory<IEnumerable<RestaurantDto>>.NotFound(
//                    "No restaurants found",
//                    new[] { new ErrorDetail("NO_RESULTS", "No restaurants within the specified radius") });
//            }

//            // Bước 3: Tính khoảng cách thực tế (driving distance) bằng OSRM
//            var drivingResults = await osrmCalculator.CalculateDistancesToPointsAsync(
//                request.Latitude,
//                request.Longitude,
//                geoResults.Select(r => (r.Id, r.Latitude, r.Longitude)).ToList(),
//                request.Unit,
//                cancellationToken);

//            if (!drivingResults.Any())
//            {
//                _logger.LogInformation("No restaurants found near ({Latitude}, {Longitude}) after OSRM calculation",
//                    request.Latitude, request.Longitude);
//                return ResponseFactory<IEnumerable<RestaurantDto>>.NotFound(
//                    "No restaurants found",
//                    new[] { new ErrorDetail("NO_RESULTS", "No restaurants within the specified radius") });
//            }

//            // Bước 4: Lấy thông tin nhà hàng tương ứng
//            var distanceDict = drivingResults.ToDictionary(r => r.Id, r => r.Distance);
//            var restaurantIds = drivingResults.Select(r => r.Id.ToString()).ToList();

//            var restaurants = await _dalContainer.DbContext.Set<Restaurant>()
//                .Where(r => restaurantIds.Contains(r.Id))
//                .ToListAsync(cancellationToken);

//            // Bước 5: Map sang DTO
//            var restaurantDtos = MapToDto(restaurants, distanceDict);

//            _logger.LogInformation("Found {Count} restaurants near ({Latitude}, {Longitude})", restaurantDtos.Count, request.Latitude, request.Longitude);
//            return ResponseFactory<IEnumerable<RestaurantDto>>.Ok(restaurantDtos, "Restaurants found");
//        }
//        catch (OperationCanceledException)
//        {
//            _logger.LogWarning("Operation canceled while finding restaurants near ({Latitude}, {Longitude})", request.Latitude, request.Longitude);
//            return ResponseFactory<IEnumerable<RestaurantDto>>.BadRequest(
//                "Operation canceled",
//                new[] { new ErrorDetail("OPERATION_CANCELED", "The request was canceled") });
//        }
//        catch (Exception ex)
//        {
//            _logger.LogError(ex, "Failed to find restaurants near ({Latitude}, {Longitude})", request.Latitude, request.Longitude);
//            return ResponseFactory<IEnumerable<RestaurantDto>>.InternalServerError(
//                "Failed to find restaurants",
//                new[] { new ErrorDetail("INTERNAL_ERROR", ex.Message) });
//        }
//    }

//    private List<RestaurantDto> MapToDto(List<Restaurant> restaurants, Dictionary<string, double?> distanceDict)
//    {
//        return restaurants.Select(r => new RestaurantDto
//        {
//            Id = r.Id,
//            Name = r.Name,
//            Address = r.Address,
//            Latitude = r.Latitude,
//            Longitude = r.Longitude,
//            Distance = distanceDict.TryGetValue(r.Id, out var distance) ? distance : 0
//        }).ToList();
//    }
//}