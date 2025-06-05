using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Common.Authorization.Attributes;

public class RequiredPermissionAttribute : Attribute
{
    public string CodeName { get; }

    public RequiredPermissionAttribute(string codeName)
    {
        CodeName = codeName ?? throw new ArgumentNullException(nameof(codeName));
    }
}