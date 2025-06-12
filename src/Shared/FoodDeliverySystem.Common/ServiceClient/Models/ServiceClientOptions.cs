namespace FoodDeliverySystem.Common.ServiceClient.Models;

public class ServiceClientOptions
{
    public Dictionary<string, string>? Headers { get; set; }
    public TimeSpan? Timeout { get; set; }
}