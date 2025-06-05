namespace FoodDeliverySystem.Common.Email.Models;

public class EmailMessage
{
    public string ToEmail { get; set; }
    public string ToName { get; set; }
    public string Subject { get; set; }
    public string Body { get; set; }
    public bool IsHtml { get; set; } = false;
    public List<string> Attachments { get; set; } = new List<string>(); // Đường dẫn đến file đính kèm (nếu có)

    public EmailMessage(string toEmail, string toName, string subject, string body, bool isHtml = false)
    {
        ToEmail = toEmail ?? throw new ArgumentNullException(nameof(toEmail));
        ToName = toName ?? string.Empty;
        Subject = subject ?? throw new ArgumentNullException(nameof(subject));
        Body = body ?? throw new ArgumentNullException(nameof(body));
        IsHtml = isHtml;
    }
}