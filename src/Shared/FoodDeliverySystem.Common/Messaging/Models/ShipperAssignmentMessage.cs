namespace FoodDeliverySystem.Common.Messaging.Models;

public class ShipperAssignmentMessage
{
    public Guid OrderId { get; set; }
    public Guid UserId { get; set; }
    public string DeliveryAddress { get; set; } = string.Empty;
    // Nullable để hỗ trợ TenantId sau này
    public string? TenantId { get; set; } = null;
}