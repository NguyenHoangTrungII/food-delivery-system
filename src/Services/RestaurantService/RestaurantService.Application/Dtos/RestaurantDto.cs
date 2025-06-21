namespace RestaurantService.Application.Dtos;

public class RestaurantDto
{
    public string Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Distance { get; set; }
}