using FoodDeliverySystem.Common.Authorization.Interfaces;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.Common.Messaging.Interfaces;
using FoodDeliverySystem.Common.Messaging.Models;
using FoodDeliverySystem.Common.Messaging.Implementations;

namespace FoodDeliverySystem.Common.Messaging;

public class PermissionChangeMessageHandler : IMessageHandler<PermissionChangeMessage>
{
    private readonly IPermissionCacheService _permissionCacheService;
    private readonly ILoggerAdapter<PermissionChangeMessageHandler> _logger;

    public PermissionChangeMessageHandler(
        IPermissionCacheService permissionCacheService,
        ILoggerAdapter<PermissionChangeMessageHandler> logger)
    {
        _permissionCacheService = permissionCacheService ?? throw new ArgumentNullException(nameof(permissionCacheService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task HandleAsync(PermissionChangeMessage message, CancellationToken cancellationToken = default)
    {
        if (message.UserId == Guid.Empty)
        {
            _logger.LogWarning("Invalid UserId in PermissionChangeMessage.");
            return;
        }

        await _permissionCacheService.InvalidateCacheAsync(message.UserId, cancellationToken);
        _logger.LogInformation("Invalidated cache for UserId {UserId}", message.UserId);
    }
}