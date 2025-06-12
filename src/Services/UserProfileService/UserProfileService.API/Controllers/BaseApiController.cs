using FoodDeliverySystem.Common.ApiResponse.Extensions;
using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Application.Dtos;

namespace UserProfileService.API.Controllers;

public abstract class BaseApiController : ControllerBase
{
    protected readonly IMediator _mediator;

    protected BaseApiController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    protected IActionResult ToActionResult<T>(ApiResponseWithData<T> response)
    {
        return this.ToActionResult(response, GetStatusCode(response));
    }

    protected static int GetStatusCode<T>(ApiResponseWithData<T> response)
    {
        return response.Status switch
        {
            "success" => response switch
            {
                ApiResponseWithData<UserProfileDto> when typeof(T) == typeof(UserProfileDto) => StatusCodes.Status201Created, // CreateUserProfile
                ApiResponseWithData<AddressDto> when typeof(T) == typeof(AddressDto) => StatusCodes.Status201Created, // AddAddress
                ApiResponseWithData<UserDeviceDto> when typeof(T) == typeof(UserDeviceDto) => StatusCodes.Status201Created, // AddUserDevice
                _ => StatusCodes.Status200OK // UpdateUserProfile, DeleteUserProfile, RemoveAddress, RemoveUserDevice, GetUserProfileById
            },
            "error" => response.Errors.FirstOrDefault()?.Code switch
            {
                "NOT_FOUND" => StatusCodes.Status404NotFound,
                "CONFLICT" => StatusCodes.Status400BadRequest,
                "VALIDATION_ERROR" => StatusCodes.Status400BadRequest,
                "UNAUTHORIZED" => StatusCodes.Status401Unauthorized,
                "INTERNAL_ERROR" => StatusCodes.Status500InternalServerError,
                _ => StatusCodes.Status400BadRequest
            },
            _ => StatusCodes.Status500InternalServerError
        };
    }
}