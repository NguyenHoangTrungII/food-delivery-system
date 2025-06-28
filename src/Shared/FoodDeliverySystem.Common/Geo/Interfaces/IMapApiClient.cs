using FoodDeliverySystem.Common.Geo.Models;

namespace FoodDeliverySystem.Common.Geo.Interfaces;

public interface IMapApiClient
{
    Task<(double Distance, TimeSpan Duration)> GetRoadDistanceAsync(double lat1, double lon1, double lat2, double lon2,
        GeoUnit unit = GeoUnit.Kilometers, CancellationToken cancellationToken = default);
}