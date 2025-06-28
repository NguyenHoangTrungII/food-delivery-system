using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Common.Weather.Models;

public class WeatherForecast
{
    public WeatherInfo[] Hourly { get; set; } = Array.Empty<WeatherInfo>();
}
