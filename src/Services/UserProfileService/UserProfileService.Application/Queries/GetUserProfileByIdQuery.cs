using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;
using UserProfileService.Application.Dtos;

namespace UserProfileService.Application.Queries;

public record GetUserProfileByIdQuery(Guid UserId) : IRequest<ApiResponseWithData<UserProfileDto>>;