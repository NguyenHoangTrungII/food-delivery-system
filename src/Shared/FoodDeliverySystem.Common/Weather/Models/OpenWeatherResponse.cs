using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Common.Weather.Models;

internal class OpenWeatherResponse
{
    public WeatherData[]? Weather { get; set; }
    public MainData? Main { get; set; }
    public RainData? Rain { get; set; }
    public WindData? Wind { get; set; }
}

internal class WeatherData
{
    public string Main { get; set; } = string.Empty;
}

internal class MainData
{
    public double Temp { get; set; }
}

internal class RainData
{
    [System.Text.Json.Serialization.JsonPropertyName("1h")]
    public double OneHour { get; set; }
}

internal class WindData
{
    public double Speed { get; set; }
}