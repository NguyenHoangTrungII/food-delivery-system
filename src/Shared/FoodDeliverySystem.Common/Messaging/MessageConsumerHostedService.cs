using FoodDeliverySystem.Common.Messaging.Interfaces;
using FoodDeliverySystem.Common.Messaging.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FoodDeliverySystem.Common.Messaging;

public class MessageConsumerHostedService<T> : BackgroundService
{
    private readonly IMessageConsumer _consumer;
    private readonly ILogger<MessageConsumerHostedService<T>> _logger;

    public MessageConsumerHostedService(
        IMessageConsumer consumer,
        ILogger<MessageConsumerHostedService<T>> logger)
    {
        _consumer = consumer ?? throw new ArgumentNullException(nameof(consumer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting consumer for {MessageType}...", typeof(T).Name);
        try
        {
            await _consumer.StartConsumingAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Consumer for {MessageType} stopped unexpectedly.", typeof(T).Name);
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping consumer for {MessageType}...", typeof(T).Name);
        if (_consumer is IDisposable disposable)
        {
            disposable.Dispose();
        }
        await base.StopAsync(cancellationToken);
    }
}