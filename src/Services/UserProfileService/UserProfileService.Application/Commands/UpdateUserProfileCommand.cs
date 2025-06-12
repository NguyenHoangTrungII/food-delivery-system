using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;
using UserProfileService.Application.Dtos;

namespace UserProfileService.Application.Commands;

public record UpdateUserProfileCommand(Guid UserId, string Name, string Email) : IRequest<ApiResponseWithData<UserProfileDto>>;