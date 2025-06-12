namespace UserProfileService.Application.Dtos;

public class AddressDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsDefault { get; set; }
}