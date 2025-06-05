namespace FoodDeliverySystem.Common.Messaging.Configuration;

public class MessageQueueConfig
{
    public string HostName { get; set; } = "localhost";
    public string QueueName { get; set; } = string.Empty;
    public string ExchangeName { get; set; } = string.Empty;
    public string RoutingKey { get; set; } = string.Empty;
    public string DeadLetterExchange { get; set; } = string.Empty;
    public string DeadLetterQueue { get; set; } = string.Empty;
}