using System;
using System.Collections.Generic;

namespace AuthService.Domain.Entities;

public class FunctionAction
{
    public Guid Id { get; set; } // Khóa chính kiểu Guid
    public string CodeName { get; set; } = string.Empty; // Mã mô tả, ví dụ: "CREATE_ORDER"
    public string Name { get; set; } = string.Empty; // Tên hiển thị, ví dụ: "Create Order"
    public string ActionType { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
    public string UrlPattern { get; set; } = string.Empty;
    public string HttpMethod { get; set; } = string.Empty;
    public string FunctionScope { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedOn { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;

    // Navigation properties
    public List<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}