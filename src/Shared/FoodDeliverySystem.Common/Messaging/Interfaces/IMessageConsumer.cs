namespace FoodDeliverySystem.Common.Messaging.Interfaces;

public interface IMessageConsumer
{
    Task StartConsumingAsync(CancellationToken cancellationToken = default);
}