using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;
using UserProfileService.Application.Dtos;

namespace UserProfileService.Application.Commands;

public record AddUserDeviceCommand(Guid UserId, string DeviceId, string DeviceName, string OperatingSystem, DateTime LastLogin) : IRequest<ApiResponseWithData<UserDeviceDto>>;