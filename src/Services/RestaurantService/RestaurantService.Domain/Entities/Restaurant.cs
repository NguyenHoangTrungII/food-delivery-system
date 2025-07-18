﻿using FoodDeliverySystem.Common.Geo.Interfaces;
using FoodDeliverySystem.DataAccess.Entities;
using System.Drawing;

namespace RestaurantService.Domain.Entities;

public class Restaurant :  IGeoEntity

{
    public string Id { get; set; } 
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public double? Distance { get; set; }
    //public Point Geom { get; set; } // Thêm property

    //public Point Geom { get; set; } 

}