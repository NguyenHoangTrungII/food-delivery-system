using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Common.Weather.Models;

public class WeatherInfo
{
    public string Condition { get; set; } = string.Empty; // e.g., "Rain", "Clear"
    public double Temperature { get; set; } // Celsius
    public double Precipitation { get; set; } // mm/hour
    public double WindSpeed { get; set; } // km/h
}

