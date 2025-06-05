using FoodDeliverySystem.Common.Authorization.Interfaces;
using FoodDeliverySystem.Common.Authorization.Models;
using FoodDeliverySystem.Common.Caching.Configuration;
using FoodDeliverySystem.Common.Caching.Services;
using FoodDeliverySystem.Common.Logging;
using StackExchange.Redis;

namespace FoodDeliverySystem.Common.Authorization.Implementations;

public class PermissionChecker : IPermissionChecker
{
    private readonly IPermissionCacheService _cacheService;
    private readonly ICacheService _rawCacheService;
    private readonly CacheOptions _cacheOptions;
    private readonly ILoggerAdapter<PermissionChecker> _logger;

    public PermissionChecker(
        IPermissionCacheService cacheService,
        ICacheService rawCacheService,
        CacheOptions cacheOptions,
        ILoggerAdapter<PermissionChecker> logger)
    {
        _cacheService = cacheService;
        _rawCacheService = rawCacheService;
        _cacheOptions = cacheOptions;
        _logger = logger;
    }

    public async Task<bool> HasPermissionAsync(PermissionCheckRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Checking permission for UserId {UserId}, FunctionId {FunctionId}", request.UserId, request.CodeName);

        try
        {
            var cachedPermission = await _cacheService.GetPermissionAsync(request, cancellationToken);
            if (cachedPermission.HasValue)
            {
                await _rawCacheService.UpdateTTLAsync($"permissions:{request.UserId}", _cacheOptions.DefaultTTL, cancellationToken);
                await _rawCacheService.UpdateTTLAsync($"session:{request.UserId}", _cacheOptions.SessionTTL, cancellationToken);
                await _rawCacheService.SetAsync($"session:tracker:{request.UserId}", DateTime.UtcNow.ToString("o"), _cacheOptions.SessionTTL, cancellationToken);
                return cachedPermission.Value;
            }

            _logger.LogWarning("Permission not found in cache for UserId {UserId}, FunctionId {FunctionId}", request.UserId, request.CodeName);
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to check permission for UserId {UserId}, FunctionId {FunctionId}", request.UserId, request.CodeName);
            throw new Exception("Permission check failed.", ex);
        }
    }
}
