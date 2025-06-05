namespace FoodDeliverySystem.Common.Email.Configuration;

public class EmailConfig
{
    public string SmtpHost { get; set; } = "smtp.gmail.com";
    public int SmtpPort { get; set; } = 587;
    public string SmtpUsername { get; set; } = string.Empty;
    public string SmtpPassword { get; set; } = string.Empty;
    public string FromEmail { get; set; } = "no-reply@fooddelivery.com";
    public string FromName { get; set; } = "Food Delivery System";
    public bool EnableSsl { get; set; } = true;
}