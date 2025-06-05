
// src/common/Authorization/Configuration/PermissionConfig.cs

namespace FoodDeliverySystem.Common.Authorization.Configuration;

public class PermissionConfig
{
    public string AuthServicePermissionEndpoint { get; set; } = "/api/permissions/check";
    public TimeSpan CacheTTL { get; set; } = TimeSpan.FromHours(1);
}