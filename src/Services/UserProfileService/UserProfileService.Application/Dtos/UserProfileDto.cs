namespace UserProfileService.Application.Dtos;

public class UserProfileDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<AddressDto> Addresses { get; set; } = new();
    public List<UserDeviceDto> Devices { get; set; } = new();
}