using AuthService.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace AuthService.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GoogleOAuthController : ControllerBase
{
    private readonly IConfiguration _configuration;

    public GoogleOAuthController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpGet("login")]
    public IActionResult Login()
    {
        var clientId = _configuration["GoogleOAuth:ClientId"];
        var redirectUri = _configuration["GoogleOAuth:RedirectUri"];
        var scope = "openid email profile";
        var state = Guid.NewGuid().ToString("N"); // CSRF protection

        var authUrl = "https://accounts.google.com/o/oauth2/v2/auth" +
                      $"?client_id={clientId}" +
                      $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                      $"&response_type=code" +
                      $"&scope={Uri.EscapeDataString(scope)}" +
                      $"&state={state}" +
                      "&access_type=offline" +
                      "&prompt=consent";

        return Redirect(authUrl);
    }

    [HttpGet("google-callback")]
    public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state, [FromQuery] string error)
    {
        if (!string.IsNullOrEmpty(error))
        {
            return BadRequest($"Google OAuth Error: {error}");
        }

        if (string.IsNullOrEmpty(code))
        {
            return BadRequest("Authorization code is missing.");
        }

        // Trao đổi code để lấy token
        var clientId = _configuration["GoogleOAuth:ClientId"];
        var clientSecret = _configuration["GoogleOAuth:ClientSecret"];
        var redirectUri = _configuration["GoogleOAuth:RedirectUri"];

        using var httpClient = new HttpClient();
        var tokenRequest = new HttpRequestMessage(HttpMethod.Post, "https://oauth2.googleapis.com/token")
        {
            Content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                { "code", code },
                { "client_id", clientId },
                { "client_secret", clientSecret },
                { "redirect_uri", redirectUri },
                { "grant_type", "authorization_code" }
            })
        };

        var tokenResponse = await httpClient.SendAsync(tokenRequest);
        if (!tokenResponse.IsSuccessStatusCode)
        {
            return BadRequest("Failed to exchange authorization code for token.");
        }

        var tokenContent = await tokenResponse.Content.ReadAsStringAsync();
        var tokenData = System.Text.Json.JsonSerializer.Deserialize<GoogleLoginResponseDto>(tokenContent, new System.Text.Json.JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });

        if (tokenData == null || string.IsNullOrEmpty(tokenData.IdToken))
        {
            return BadRequest("Failed to retrieve ID token from Google.");
        }

        // Chuyển hướng về frontend với ID Token
        // Giả định frontend sẽ gọi API khác để hoàn tất đăng nhập
        var redirectUrl = $"http://localhost:3000/auth/google-callback?id_token={tokenData.IdToken}";
        return Redirect(redirectUrl);
    }
}