namespace UserProfileService.Application.Dtos;

public class UserDeviceDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string DeviceId { get; set; } = string.Empty;
    public string DeviceName { get; set; } = string.Empty;
    public string OperatingSystem { get; set; } = string.Empty;
    public DateTime LastLogin { get; set; }
}