namespace AuthService.Application.Dtos;

public class GoogleLoginResponseDto
{
    public string IdToken { get; set; } = string.Empty; // ID Token từ Google
    public string AccessToken { get; set; } = string.Empty; // Access Token từ Google
}