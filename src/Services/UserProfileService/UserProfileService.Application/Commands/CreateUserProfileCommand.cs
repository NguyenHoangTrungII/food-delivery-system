using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;

namespace UserProfileService.Application.Commands;

public record CreateUserProfileCommand(Guid UserId, string Name, string Email) : IRequest<ApiResponseWithData<Guid>>;