namespace FoodDeliverySystem.Common.ServiceClient.Configuration;

public class ServiceClientConfig
{
    public string BaseAddress { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
    public int MaxRetryAttempts { get; set; } = 3;
    public int RetryDelayMilliseconds { get; set; } = 500;

    // Thêm phương thức để kiểm tra cấu hình hợp lệ
    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(BaseAddress))
            throw new InvalidOperationException("ApiGateway.BaseAddress must be configured in appsettings.json.");
        if (!Uri.TryCreate(BaseAddress, UriKind.Absolute, out _))
            throw new InvalidOperationException("ApiGateway.BaseAddress must be a valid absolute URL.");
        if (TimeoutSeconds <= 0)
            throw new InvalidOperationException("TimeoutSeconds must be greater than 0.");
        if (MaxRetryAttempts < 0)
            throw new InvalidOperationException("MaxRetryAttempts cannot be negative.");
        if (RetryDelayMilliseconds < 0)
            throw new InvalidOperationException("RetryDelayMilliseconds cannot be negative.");
    }
}