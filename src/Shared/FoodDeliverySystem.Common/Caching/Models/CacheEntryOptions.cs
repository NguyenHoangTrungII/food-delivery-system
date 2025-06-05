using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Common.Caching.Models;

public class CacheEntryOptions
{
    public TimeSpan? AbsoluteExpiration { get; set; } // Thời gian hết hạn tuyệt đối
    public TimeSpan? SlidingExpiration { get; set; } // Thời gian hết hạn trượt
}
