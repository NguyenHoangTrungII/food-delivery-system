using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Common.Authorization.Models;
public class PermissionCheckRequest
{
    public Guid UserId { get; set; }
    public Guid FunctionId { get; set; }
    //public string? TenantId { get; set; }
    public string CodeName { get; set; }
}