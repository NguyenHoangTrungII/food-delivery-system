using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;

namespace UserProfileService.Application.Commands;

public record RemoveAddressCommand(Guid UserId, Guid AddressId) : IRequest<ApiResponseWithData<bool>>;