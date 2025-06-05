
namespace AuthService.Application.Dtos;

public class RolePermissionDto
{
    public Guid Id { get; set; }
    public Guid RoleId { get; set; }
    public Guid FunctionID { get; set; } 
    public bool Allowed { get; set; }
}