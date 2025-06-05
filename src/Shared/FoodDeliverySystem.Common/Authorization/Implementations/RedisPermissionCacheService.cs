// src/common/Authorization/Implementations/RedisPermissionCacheService.cs
using FoodDeliverySystem.Common.Authorization.Interfaces;
using FoodDeliverySystem.Common.Authorization.Models;
using FoodDeliverySystem.Common.Caching.Configuration;
using FoodDeliverySystem.Common.Caching.Services;
using FoodDeliverySystem.Common.Logging;

namespace FoodDeliverySystem.Common.Authorization.Implementations;

public class RedisPermissionCacheService : IPermissionCacheService
{
    private readonly ICacheService _cacheService;
    private readonly CacheOptions _cacheOptions;
    private readonly ILoggerAdapter<RedisPermissionCacheService> _logger;

    public RedisPermissionCacheService(ICacheService cacheService, CacheOptions cacheOptions, ILoggerAdapter<RedisPermissionCacheService> logger)
    {
        _cacheService = cacheService;
        _cacheOptions = cacheOptions;
        _logger = logger;
    }

    //public async Task<bool?> GetPermissionAsync(PermissionCheckRequest request, CancellationToken cancellationToken = default)
    //{
    //    var cacheKey = $"permissions:{request.UserId}";
    //    var field = request.FunctionId.ToString();
    //    return await _cacheService.HashGetAsync(cacheKey, field, cancellationToken);
    //}

    public async Task<bool?> GetPermissionAsync(PermissionCheckRequest request, CancellationToken cancellationToken = default)
    {
        var key = $"permissions:{request.UserId}";
        var field = request.CodeName; // Dùng CodeName thay vì FunctionId
        return await  _cacheService.HashGetAsync(key, field);
    }

    public async Task SetPermissionAsync(PermissionCheckRequest request, bool hasPermission, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"permissions:{request.UserId}";
        var field = request.FunctionId.ToString();
        await _cacheService.HashSetAsync(cacheKey, field, hasPermission, _cacheOptions.PermissionTTL, cancellationToken);
        _logger.LogInformation("Cached permission for UserId {UserId}, FunctionId {FunctionId}: {HasPermission}", request.UserId, field, hasPermission);
    }

    public async Task InvalidateCacheAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var cacheKey = $"permissions:{userId}";
        await _cacheService.RemoveAsync(cacheKey, cancellationToken);
        _logger.LogInformation("Invalidated permission cache for UserId {UserId}", userId);
    }
}