using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;

namespace UserProfileService.Application.Commands;

public record DeleteUserProfileCommand(Guid UserId) : IRequest<ApiResponseWithData<bool>>;