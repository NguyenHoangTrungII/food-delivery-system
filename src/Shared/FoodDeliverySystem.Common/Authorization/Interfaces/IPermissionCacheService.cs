// src/common/Authorization/Interfaces/IPermissionCacheService.cs
using FoodDeliverySystem.Common.Authorization.Models;

namespace FoodDeliverySystem.Common.Authorization.Interfaces;

public interface IPermissionCacheService
{
    Task<bool?> GetPermissionAsync(PermissionCheckRequest request, CancellationToken cancellationToken = default);
    Task SetPermissionAsync(PermissionCheckRequest request, bool hasPermission, CancellationToken cancellationToken = default);
    Task InvalidateCacheAsync(Guid userId, CancellationToken cancellationToken = default);
}