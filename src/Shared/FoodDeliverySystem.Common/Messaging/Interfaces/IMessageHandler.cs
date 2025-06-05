namespace FoodDeliverySystem.Common.Messaging.Interfaces;

public interface IMessageHandler<T>
{
    Task HandleAsync(T message, CancellationToken cancellationToken = default);
}