namespace FoodDeliverySystem.Common.Geo.Configurations;

public class FeeConfig
{
    public decimal BaseFee { get; set; } = 2.0m; // Phí cố định
    public decimal PerKmFee { get; set; } = 0.5m; // Phí mỗi km
    public Dictionary<TimeSpan, decimal> SurgePricing { get; set; } = new(); // Phí giờ cao điểm
    public decimal WeatherSurcharge { get; set; } = 0.5m; // Phí thời tiết xấu
}

//public class FeeConfig
//{
//    public decimal BaseFee { get; set; }
//    public decimal PerKmFee { get; set; }
//    public Dictionary<TimeSpan, decimal> SurgePricing { get; set; }
//}
