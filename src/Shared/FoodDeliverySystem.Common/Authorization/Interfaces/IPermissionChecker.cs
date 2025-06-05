// src/common/Authorization/Interfaces/IPermissionChecker.cs
using FoodDeliverySystem.Common.Authorization.Models;

namespace FoodDeliverySystem.Common.Authorization.Interfaces;

public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(PermissionCheckRequest request, CancellationToken cancellationToken = default);
}