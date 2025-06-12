using FoodDeliverySystem.DataAccess.Entities;
using System.Net;

namespace UserProfileService.Domain.Entities;

public class UserProfile : BaseEntity
{
    public Guid UserId { get; set; } // Liên kết với User trong AuthService
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<Address> Addresses { get; set; } = new();
    public List<UserDevice> Devices { get; set; } = new();
    //public DateTime CreatedAt { get; set; }
    //public string CreatedBy { get; set; } = string.Empty;
    //public DateTime? ModifiedAt { get; set; }
    //public string? ModifiedBy { get; set; }
}