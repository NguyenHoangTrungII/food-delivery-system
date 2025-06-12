using FoodDeliverySystem.Common.ApiResponse.Models;
using MediatR;
using UserProfileService.Application.Dtos;

namespace UserProfileService.Application.Commands;

public record AddAddressCommand(Guid UserId, string Street, string City, string PostalCode, double Latitude, double Longitude, bool IsDefault) : IRequest<ApiResponseWithData<AddressDto>>;