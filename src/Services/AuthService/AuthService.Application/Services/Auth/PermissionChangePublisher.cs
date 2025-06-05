
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.Common.Messaging.Configuration;
using FoodDeliverySystem.Common.Messaging.Models;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using System.Text.Json;

namespace AuthService.Application.Services.Auth;

public class PermissionChangePublisher : IDisposable
{
    private readonly MessageQueueConfig _config;
    private readonly ILoggerAdapter<PermissionChangePublisher> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;

    public PermissionChangePublisher(IOptions<MessageQueueConfig> config, ILoggerAdapter<PermissionChangePublisher> logger)
    {
        _config = config.Value ?? throw new ArgumentNullException(nameof(config));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        var factory = new ConnectionFactory { HostName = _config.HostName, AutomaticRecoveryEnabled = true };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _channel.ExchangeDeclare(_config.ExchangeName, ExchangeType.Topic, durable: true);
    }

    public async Task PublishPermissionChangeAsync(Guid userId)
    {
        try
        {
            var message = new PermissionChangeMessage { UserId = userId };
            var body = JsonSerializer.SerializeToUtf8Bytes(message);

            await Task.Run(() =>
            {
                _channel.BasicPublish(
                    exchange: _config.ExchangeName,
                    routingKey: _config.RoutingKey,
                    basicProperties: null,
                    body: body);
            });

            _logger.LogInformation("Published permission change for UserId {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish permission change for UserId {UserId}", userId);
            throw;
        }

        // Để hỗ trợ TenantId sau này
        /*
        public async Task PublishPermissionChangeAsync(Guid userId, string? tenantId)
        {
            var message = new PermissionChangeMessage { UserId = userId, TenantId = tenantId };
            // ... giữ logic publish như trên ...
            _logger.LogInformation("Published permission change for UserId {UserId}, TenantId {TenantId}", userId, tenantId ?? "N/A");
        }
        */
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}