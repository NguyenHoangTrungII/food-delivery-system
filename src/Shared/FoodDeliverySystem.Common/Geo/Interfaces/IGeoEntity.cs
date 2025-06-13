namespace FoodDeliverySystem.Common.Geo.Interfaces;

public interface IGeoEntity
{
    string Id { get; }
    double Latitude { get; }
    double Longitude { get; }
    double Distance { get; set; }
}
