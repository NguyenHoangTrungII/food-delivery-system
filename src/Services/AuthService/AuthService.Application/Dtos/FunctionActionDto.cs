
namespace AuthService.Application.Dtos;

public class FunctionActionDto
{
    public string FunctionID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ActionType { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string UrlPattern { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string FunctionScope { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}