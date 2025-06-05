using FoodDeliverySystem.Common.ApiResponse.Models;
using FoodDeliverySystem.Common.Email.Models;

namespace FoodDeliverySystem.Common.Email.Interfaces;

public interface IEmailService
{
    Task<ApiResponseWithData<bool>> SendEmailAsync(EmailMessage message, CancellationToken cancellationToken = default);
}