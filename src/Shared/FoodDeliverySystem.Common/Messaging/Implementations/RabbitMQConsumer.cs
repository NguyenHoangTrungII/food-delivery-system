using FoodDeliverySystem.Common.Authorization.Interfaces;
using FoodDeliverySystem.Common.Messaging.Configuration;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text.Json;
using Polly;
using Polly.Retry;
using FoodDeliverySystem.Common.Logging;
using FoodDeliverySystem.Common.Messaging.Interfaces;
using Microsoft.Extensions.Options;

namespace FoodDeliverySystem.Common.Messaging.Implementations;

public class RabbitMQConsumer<T> : IMessageConsumer, IDisposable
{
    private readonly IMessageHandler<T> _messageHandler;
    private readonly MessageQueueConfig _config;
    private readonly ILoggerAdapter<RabbitMQConsumer<T>> _logger;
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly AsyncRetryPolicy _retryPolicy;

    //public RabbitMQConsumer(
    //    IMessageHandler<T> messageHandler,
    //    MessageQueueConfig config,
    //    ILoggerAdapter<RabbitMQConsumer<T>> logger)
    //{
    //    _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
    //    _config = config ?? throw new ArgumentNullException(nameof(config));
    //    _logger = logger ?? throw new ArgumentNullException(nameof(logger));

    //    var factory = new ConnectionFactory { HostName = _config.HostName, AutomaticRecoveryEnabled = true };
    //    _connection = factory.CreateConnection();
    //    _channel = _connection.CreateModel();

    //    _retryPolicy = Policy
    //        .Handle<Exception>()
    //        .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
    //            (ex, time) => _logger.LogWarning("Retry after {TimeSpan} due to {Exception}", time, ex.Message));
    //}

    public RabbitMQConsumer(
    IMessageHandler<T> messageHandler,
    IOptions<MessageQueueConfig> options,
    ILoggerAdapter<RabbitMQConsumer<T>> logger)
    {
        _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
        _config = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        var factory = new ConnectionFactory { HostName = _config.HostName, AutomaticRecoveryEnabled = true };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();

        _retryPolicy = Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                (ex, time) => _logger.LogWarning("Retry after {TimeSpan} due to {Exception}", time, ex.Message));
    }

    public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
    {
        _channel.ExchangeDeclare(_config.DeadLetterExchange, ExchangeType.Topic, durable: true);
        _channel.QueueDeclare(_config.DeadLetterQueue, durable: true, exclusive: false, autoDelete: false);
        _channel.QueueBind(_config.DeadLetterQueue, _config.DeadLetterExchange, _config.RoutingKey);

        _channel.ExchangeDeclare(_config.ExchangeName, ExchangeType.Topic, durable: true);
        var arguments = new Dictionary<string, object>
        {
            { "x-dead-letter-exchange", _config.DeadLetterExchange },
            { "x-dead-letter-routing-key", _config.RoutingKey }
        };
        _channel.QueueDeclare(_config.QueueName, durable: true, exclusive: false, autoDelete: false, arguments);
        _channel.QueueBind(_config.QueueName, _config.ExchangeName, _config.RoutingKey);
        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

        var consumer = new EventingBasicConsumer(_channel);
        consumer.Received += async (model, ea) =>
        {
            try
            {
                var body = ea.Body.ToArray();
                var message = JsonSerializer.Deserialize<T>(body);

                if (message == null)
                {
                    _logger.LogWarning("Invalid message received for type {MessageType}.", typeof(T).Name);
                    _channel.BasicAck(ea.DeliveryTag, false);
                    return;
                }

                _logger.LogInformation("Received message for type {MessageType}", typeof(T).Name);

                await _retryPolicy.ExecuteAsync(async () =>
                {
                    await _messageHandler.HandleAsync(message, cancellationToken);
                });

                _channel.BasicAck(ea.DeliveryTag, false);
                _logger.LogInformation("Processed message for type {MessageType}", typeof(T).Name);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process message for type {MessageType}", typeof(T).Name);
                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
            }
        };

        _channel.BasicConsume(queue: _config.QueueName, autoAck: false, consumer: consumer);
        _logger.LogInformation("Started consuming messages from queue {QueueName}", _config.QueueName);

        await Task.Delay(Timeout.Infinite, cancellationToken);
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        _channel?.Dispose();
        _connection?.Dispose();
    }
}

//using FoodDeliverySystem.Common.Authorization.Interfaces;
//using FoodDeliverySystem.Common.Authorization.Interfaces;
//using FoodDeliverySystem.Common.Messaging.Configuration;
//using FoodDeliverySystem.Common.Messaging.Models;
//using RabbitMQ.Client;
//using RabbitMQ.Client.Events;
//using System.Text.Json;
//using Polly;
//using Polly.Retry;
//using FoodDeliverySystem.Common.Messaging.Interfaces;
//using FoodDeliverySystem.Common.Logging;
////using Microsoft.EntityFrameworkCore.Metadata;

//namespace FoodDeliverySystem.Common.Messaging.Implementations;

//public class RabbitMQConsumer : IMessageConsumer, IDisposable
//{
//    private readonly IPermissionCacheService _permissionCacheService;
//    private readonly MessageQueueConfig _config;
//    private readonly ILoggerAdapter<RabbitMQConsumer> _logger;
//    private readonly IConnection _connection;
//    private readonly IModel _channel;
//    private readonly AsyncRetryPolicy _retryPolicy;

//    public RabbitMQConsumer(
//        IPermissionCacheService permissionCacheService,
//        MessageQueueConfig config,
//        ILoggerAdapter<RabbitMQConsumer> logger)
//    {
//        _permissionCacheService = permissionCacheService ?? throw new ArgumentNullException(nameof(permissionCacheService));
//        _config = config ?? throw new ArgumentNullException(nameof(config));
//        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

//        // Khởi tạo connection bền vững
//        var factory = new ConnectionFactory { HostName = _config.HostName, AutomaticRecoveryEnabled = true };
//        _connection = factory.CreateConnection();
//        _channel = _connection.CreateModel();

//        // Retry policy cho xử lý message
//        _retryPolicy = Policy
//            .Handle<Exception>()
//            .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
//                (ex, time) => _logger.LogWarning("Retry after {TimeSpan} due to {Exception}", time, ex.Message));
//    }

//    public async Task StartConsumingAsync(CancellationToken cancellationToken = default)
//    {
//        // Khai báo dead letter exchange và queue
//        _channel.ExchangeDeclare(_config.DeadLetterExchange, ExchangeType.Topic, durable: true);
//        _channel.QueueDeclare(_config.DeadLetterQueue, durable: true, exclusive: false, autoDelete: false);
//        _channel.QueueBind(_config.DeadLetterQueue, _config.DeadLetterExchange, _config.RoutingKey);

//        // Khai báo exchange và queue chính
//        _channel.ExchangeDeclare(_config.ExchangeName, ExchangeType.Topic, durable: true);
//        var arguments = new Dictionary<string, object>
//        {
//            { "x-dead-letter-exchange", _config.DeadLetterExchange },
//            { "x-dead-letter-routing-key", _config.RoutingKey }
//        };
//        _channel.QueueDeclare(_config.QueueName, durable: true, exclusive: false, autoDelete: false, arguments);
//        _channel.QueueBind(_config.QueueName, _config.ExchangeName, _config.RoutingKey);
//        _channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

//        var consumer = new EventingBasicConsumer(_channel);
//        consumer.Received += async (model, ea) =>
//        {
//            try
//            {
//                var body = ea.Body.ToArray();
//                var message = JsonSerializer.Deserialize<PermissionChangeMessage>(body);

//                if (message == null || message.UserId == Guid.Empty)
//                {
//                    _logger.LogWarning("Invalid permission change message received.");
//                    _channel.BasicAck(ea.DeliveryTag, false);
//                    return;
//                }

//                _logger.LogInformation("Received permission change for UserId {UserId}", message.UserId);

//                await _retryPolicy.ExecuteAsync(async () =>
//                {
//                    await _permissionCacheService.InvalidateCacheAsync(message.UserId, cancellationToken);
//                });

//                _channel.BasicAck(ea.DeliveryTag, false);
//                _logger.LogInformation("Invalidated cache for UserId {UserId}", message.UserId);

//                // Để hỗ trợ TenantId sau này
//                /*
//                if (string.IsNullOrWhiteSpace(message.TenantId))
//                {
//                    _logger.LogWarning("Missing TenantId for UserId {UserId}", message.UserId);
//                    _channel.BasicAck(ea.DeliveryTag, false);
//                    return;
//                }
//                await _permissionCacheService.InvalidateCacheAsync(message.UserId, message.TenantId, cancellationToken);
//                _logger.LogInformation("Invalidated cache for UserId {UserId}, TenantId {TenantId}", 
//                    message.UserId, message.TenantId);
//                */
//            }
//            catch (Exception ex)
//            {
//                _logger.LogError(ex, "Failed to process permission change message for delivery tag {DeliveryTag}", ea.DeliveryTag);
//                // Sau 3 lần retry thất bại, gửi message vào DLQ
//                _channel.BasicNack(ea.DeliveryTag, multiple: false, requeue: false);
//            }
//        };

//        _channel.BasicConsume(queue: _config.QueueName, autoAck: false, consumer: consumer);
//        _logger.LogInformation("Started consuming messages from queue {QueueName}", _config.QueueName);

//        // Giữ consumer chạy
//        await Task.Delay(Timeout.Infinite, cancellationToken);
//    }

//    public void Dispose()
//    {
//        _channel?.Close();
//        _connection?.Close();
//        _channel?.Dispose();
//        _connection?.Dispose();
//    }
//}