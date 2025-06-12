namespace AuthService.Application.Dtos;

public class RegisterResponseDto
{
    public Guid UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string Token { get; set; }
    public string RefreshToken { get; set; }
    public DateTime Expiry { get; set; }
}