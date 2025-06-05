using AuthService.Domain.Entities;
using EffiAP.Domain.SeedWork;
using FoodDeliverySystem.Common.ApiResponse.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AuthService.Application.Services.jwt
{
    public interface IRefreshTokenService : IScopedService
    {

        Task<ApiResponseWithData<RefreshToken>> GenerateRefreshTokenAsync(User user);

    }
}
