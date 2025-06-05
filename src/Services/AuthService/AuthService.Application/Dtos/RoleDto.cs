
namespace AuthService.Application.Dtos;

public class RoleDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool Administrator { get; set; }
}