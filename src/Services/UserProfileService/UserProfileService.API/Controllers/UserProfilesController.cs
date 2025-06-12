using FoodDeliverySystem.Common.Authorization.Attributes;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserProfileService.Application.Commands;
using UserProfileService.Application.Dtos;
using UserProfileService.Application.Queries;

namespace UserProfileService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserProfilesController : BaseApiController
{
    public UserProfilesController(IMediator mediator) : base(mediator)
    {
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> CreateUserProfile([FromBody] CreateUserProfileCommand command)
    {
        var response = await _mediator.Send(command);
        return ToActionResult(response);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> UpdateUserProfile([FromBody] UpdateUserProfileCommand command)
    {
        var response = await _mediator.Send(command);
        return ToActionResult(response);
    }

    [HttpDelete("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> DeleteUserProfile(Guid userId)
    {
        var command = new DeleteUserProfileCommand(userId);
        var response = await _mediator.Send(command);
        return ToActionResult(response);
    }

    [HttpPost("address")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddAddress([FromBody] AddAddressCommand command)
    {
        var response = await _mediator.Send(command);
        return ToActionResult(response);
    }

    [HttpDelete("address/{userId}/{addressId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveAddress(Guid userId, Guid addressId)
    {
        var command = new RemoveAddressCommand(userId, addressId);
        var response = await _mediator.Send(command);
        return ToActionResult(response);
    }

    [HttpPost("device")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> AddUserDevice([FromBody] AddUserDeviceCommand command)
    {
        var response = await _mediator.Send(command);
        return ToActionResult(response);
    }

    [HttpDelete("device/{userId}/{deviceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RemoveUserDevice(Guid userId, Guid deviceId)
    {
        var command = new RemoveUserDeviceCommand(userId, deviceId);
        var response = await _mediator.Send(command);
        return ToActionResult(response);
    }

    [HttpGet("{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserProfileById(Guid userId)
    {
        var query = new GetUserProfileByIdQuery(userId);
        var response = await _mediator.Send(query);
        return ToActionResult(response);
    }
}