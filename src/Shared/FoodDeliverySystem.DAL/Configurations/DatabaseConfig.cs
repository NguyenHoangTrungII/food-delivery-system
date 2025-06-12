namespace FoodDeliverySystem.DataAccess.Configurations;

public class DatabaseConfig
{
    public string DatabaseType { get; set; } = "SqlServer";
    public string ConnectionString { get; set; } = string.Empty;
    public int MaxRetryAttempts { get; set; } = 3;
    public int CommandTimeout { get; set; } = 30;
}