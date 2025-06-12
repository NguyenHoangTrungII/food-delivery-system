using FoodDeliverySystem.DataAccess.Entities;

namespace UserProfileService.Domain.Entities;

public class Address : BaseEntity
{
    //public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Street { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool IsDefault { get; set; }
    //public DateTime CreatedAt { get; set; }

    public UserProfile UserProfile { get; set; } = null!;
}