namespace AuthService.Application.Dtos;

public class LoginResponseDto
{
    public string Token { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty; // Thêm RefreshToken
    public DateTime Expiry { get; set; }
}