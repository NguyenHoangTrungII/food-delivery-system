namespace FoodDeliverySystem.Common.Geo.Interfaces;

// NEW: Giao diện cho geocoding
public interface IGeocodingService
{
    Task<(double Lat, double Lon)> GeocodeAsync(string address, CancellationToken cancellationToken);
    Task<string> ReverseGeocodeAsync(double lat, double lon, CancellationToken cancellationToken);
}