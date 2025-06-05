using FoodDeliverySystem.Common.ApiResponse.Factories;
using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Email.Configuration;
using FoodDeliverySystem.Common.Email.Interfaces;
using FoodDeliverySystem.Common.Email.Models;
using FoodDeliverySystem.Common.Logging;
using Microsoft.Extensions.Options;
using System.Net.Mail;
using System.Net;

namespace FoodDeliverySystem.Common.Email.Services;

public class SmtpEmailService : IEmailService
{
    private readonly EmailConfig _emailConfig;
    private readonly ILoggerAdapter<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailConfig> emailConfigOptions, ILoggerAdapter<SmtpEmailService> logger)
    {
        _emailConfig = emailConfigOptions.Value;
        _logger = logger;
    }

    public async Task<ApiResponseWithData<bool>> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Sending email to {ToEmail} with subject {Subject}", message.ToEmail, message.Subject);

            using var smtpClient = new SmtpClient(_emailConfig.SmtpHost, _emailConfig.SmtpPort)
            {
                EnableSsl = _emailConfig.EnableSsl,
                Credentials = new NetworkCredential(_emailConfig.SmtpUsername, _emailConfig.SmtpPassword)
            };

            using var mailMessage = new MailMessage
            {
                From = new MailAddress(_emailConfig.FromEmail, _emailConfig.FromName),
                Subject = message.Subject,
                Body = message.Body,
                IsBodyHtml = message.IsHtml
            };

            mailMessage.To.Add(new MailAddress(message.ToEmail, message.ToName));

            // Thêm attachments nếu có
            foreach (var attachmentPath in message.Attachments)
            {
                if (File.Exists(attachmentPath))
                {
                    mailMessage.Attachments.Add(new Attachment(attachmentPath));
                }
                else
                {
                    _logger.LogWarning("Attachment file {AttachmentPath} not found.", attachmentPath);
                }
            }

            await smtpClient.SendMailAsync(mailMessage, cancellationToken);
            _logger.LogInformation("Email sent successfully to {ToEmail}", message.ToEmail);

            return ResponseFactory<bool>.Ok(true, "Email sent successfully.");
        }
        catch (SmtpException ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail} due to SMTP error.", message.ToEmail);
            return ResponseFactory<bool>.InternalServerError($"Failed to send email due to SMTP error: {ex.Message}", new[] { new ErrorDetail("SMTP_ERROR", ex.Message) });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}.", message.ToEmail);
            return ResponseFactory<bool>.InternalServerError($"Failed to send email: {ex.Message}", new[] { new ErrorDetail("EMAIL_ERROR", ex.Message) });
        }
    }
}