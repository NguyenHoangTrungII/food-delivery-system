namespace AuthService.Application.Dtos;

public class FunctionModuleDto
{
    public string FunctionModuleID { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ParentID { get; set; }
    public int Level { get; set; }
    public string ModuleType { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public List<FunctionModuleDto> Children { get; set; } = new List<FunctionModuleDto>();
}
