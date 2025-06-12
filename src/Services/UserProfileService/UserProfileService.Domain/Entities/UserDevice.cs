using FoodDeliverySystem.DataAccess.Entities;

namespace UserProfileService.Domain.Entities;

public class UserDevice : BaseEntity
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public DateTime LastLogin { get; set; }
    public UserProfile UserProfile { get; set; } = null!;
}