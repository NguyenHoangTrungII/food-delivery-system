using AuthService.Domain.Entities;
using EffiAP.Domain.SeedWork;
using FoodDeliverySystem.Common.ApiResponse.Models;
using System.Threading.Tasks;

namespace AuthService.Application.Services;

public interface IJwtService : IScopedService
{
    Task<ApiResponseWithData<string>> GenerateJwtTokenAsync(User user);
    Task<ApiResponseWithData<User>> ValidateJwtTokenAsync(string token);

    RefreshToken GenerateRefreshToken(User user);

    string GenerateRefreshToken();
}

//using AuthService.Domain.Entities;
//using EffiAP.Domain.SeedWork;
//using System.Threading.Tasks;

//namespace AuthService.Application.Services;

//public interface IJwtService: IScopedService
//{
//    Task<string> GenerateJwtTokenAsync(User user);
//    Task<User> ValidateJwtTokenAsync(string token);
//    //Task<string> GenerateRefreshToken(User user);
//    string GenerateJwtToken(User user);

//    RefreshToken GenerateRefreshToken(User user);

//}