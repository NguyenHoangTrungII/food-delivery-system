using AuthService.Application.Dtos;
using FoodDeliverySystem.Common.ApiResponse.Extensions;
using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

public abstract class BaseApiController : ControllerBase
{

    protected readonly IMediator _mediator;

    protected BaseApiController(IMediator mediator)
    {
        this._mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
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
                ApiResponseWithData<UserDto> when typeof(T) == typeof(UserDto) => StatusCodes.Status201Created, // Register
                _ => StatusCodes.Status200OK // Login, GoogleLogin, RefreshToken, ForgotPassword, ResetPassword, Logout
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