using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Geo.Models;
using MediatR;
using RestaurantService.Application.Dtos;

namespace  RestaurantService.Application.Queries;

public class FindRestaurantsNearbyQuery : IRequest<ApiResponseWithData<IEnumerable<RestaurantDto>>>
{
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double Radius { get; set; }
    public GeoUnit Unit { get; set; }

    public FindRestaurantsNearbyQuery(double latitude, double longitude, double radius, GeoUnit unit)
    {
        Latitude = latitude;
        Longitude = longitude;
        Radius = radius;
        Unit = unit;
    }
}