//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace FoodDeliverySystem.Common.Caching.Configuration
//{
//    internal class CacheOptions
//    {
//    }
//}


// src/common/Caching/Models/CacheOptions.cs
namespace FoodDeliverySystem.Common.Caching.Configuration;


public class CacheOptions
{
    public TimeSpan PermissionTTL { get; set; } = TimeSpan.FromHours(1);
    public TimeSpan SessionTTL { get; set; } = TimeSpan.FromHours(1);
    public TimeSpan BranchTTL { get; set; } = TimeSpan.FromHours(24);
    public TimeSpan CartTTL { get; set; } = TimeSpan.FromHours(24);
    public TimeSpan DefaultTTL { get; set; } = TimeSpan.FromHours(1);
    public TimeSpan GeoDistanceTTL { get; set; } = TimeSpan.FromMinutes(30); // TTL cho GeoDistance


}