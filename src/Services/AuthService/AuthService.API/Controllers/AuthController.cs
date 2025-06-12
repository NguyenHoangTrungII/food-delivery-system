using AuthService.Application.Commands.Auth;
using AuthService.Application.Dtos;
using AuthService.Application.Dtos.Auth.Request;
using FoodDeliverySystem.Common.Authorization.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    public AuthController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var command = new LoginCommand(request.Username, request.Password);
        var response = await _mediator.Send(command);
        return ToActionResult(response);
    }

    [HttpPost("google-login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GoogleLogin([FromBody] GoogleLoginRequestDto request)
    {
        var command = new GoogleLoginCommand(request.IdToken);
        var response = await _mediator.Send(command);
        return ToActionResult(response);
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    //[RequiredPermission("CREATE_ORDER")]

    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var command = new RegisterCommand(request.Username, request.Email, request.Password);
        var response = await _mediator.Send(command);
        return ToActionResult(response);
    }

    [HttpPost("refresh-token")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
    {
        var command = new RefreshTokenCommand(request.RefreshToken);
        var response = await _mediator.Send(command);
        return ToActionResult(response);
    }

    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto request)
    {
        var command = new ForgotPasswordCommand(request.Email);
        var response = await _mediator.Send(command);
        return ToActionResult(response);
    }

    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDto request)
    {
        var command = new ResetPasswordCommand(request.Email, request.Token, request.NewPassword);
        var response = await _mediator.Send(command);
        return ToActionResult(response);
    }

    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequestDto request)
    {
        var command = new LogoutCommand(request.RefreshToken);
        var response = await _mediator.Send(command);
        return ToActionResult(response);
    }
}