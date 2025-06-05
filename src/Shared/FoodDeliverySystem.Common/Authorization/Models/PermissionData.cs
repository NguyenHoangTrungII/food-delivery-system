namespace FoodDeliverySystem.Common.Authorization.Models;

public class PermissionData
{
    public Guid Id { get; set; }
    public string CodeName { get; set; } = string.Empty;
    public bool HasPermission { get; set; }
}

//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FoodDeliverySystem.Common.Authorization.Models;

//public class PermissionData
//{
//    public Dictionary<Guid, bool> Permissions { get; set; } = new();
//}
