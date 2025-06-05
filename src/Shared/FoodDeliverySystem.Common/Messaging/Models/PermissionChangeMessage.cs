namespace FoodDeliverySystem.Common.Messaging.Models;

public class PermissionChangeMessage
{
    public Guid UserId { get; set; }
    // Nullable để dễ thêm multi-tenant sau này
    public string? TenantId { get; set; } = null;
}