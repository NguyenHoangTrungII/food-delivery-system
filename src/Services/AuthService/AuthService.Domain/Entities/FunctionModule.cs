using System;
using System.Collections.Generic;

namespace AuthService.Domain.Entities;

public class FunctionModule
{
    public Guid Id { get; set; } // Khóa chính kiểu Guid
    public string CodeName { get; set; } = string.Empty; // Mã mô tả, ví dụ: "ORDER_MANAGEMENT"
    public string Name { get; set; } = string.Empty; // Tên hiển thị, ví dụ: "Order Management"
    public Guid? ParentId { get; set; } // Khóa ngoại kiểu Guid, tham chiếu đến Id của FunctionModule cha
    public int Level { get; set; }
    public string ModuleType { get; set; } = string.Empty; // M (Module), G (Group), S (System)
    public bool IsActive { get; set; }
    public DateTime CreatedOn { get; set; }
    public string CreatedBy { get; set; } = string.Empty;
    public DateTime? ModifiedOn { get; set; }
    public string ModifiedBy { get; set; } = string.Empty;

    // Navigation properties
    public FunctionModule? Parent { get; set; }
    public List<FunctionModule> Children { get; set; } = new List<FunctionModule>();
}