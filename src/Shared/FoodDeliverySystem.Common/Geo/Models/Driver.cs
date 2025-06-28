using FoodDeliverySystem.Common.Geo.Common.Enums;
using FoodDeliverySystem.Common.Geo.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Common.Geo.Models;

public class Driver : IGeoEntity
{
    public string Id { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Distance { get; set; }
    public DriverStatus Status { get; set; }
    public TransportMode VehicleType { get; set; }
}