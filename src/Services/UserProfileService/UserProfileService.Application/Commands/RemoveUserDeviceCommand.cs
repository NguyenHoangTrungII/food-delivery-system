using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;

namespace UserProfileService.Application.Commands;

public record RemoveUserDeviceCommand(Guid UserId, Guid DeviceId) : IRequest<ApiResponseWithData<bool>>;