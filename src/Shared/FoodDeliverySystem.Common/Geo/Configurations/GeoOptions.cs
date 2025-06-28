using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Common.Geo.Configurations;

public class GeoOptions
{
    public GeoEngineConfig Engine { get; set; }
    public MapProviderConfig MapProvider { get; set; }
    public RedisConfig Redis { get; set; }
    public MongoConfig Mongo { get; set; }
    public PostGISConfig PostGIS { get; set; }
    public OSRMConfig OSRM { get; set; }
    public GoogleMapsConfig GoogleMaps { get; set; }
    public MapboxConfig Mapbox { get; set; }

    public class GeoEngineConfig
    {
        public string? Engine { get; set; }
    }

    public class RedisConfig
    {
        public string? RedisConnectionString { get; set; }
        public TimeSpan GeoDistanceTTL { get; set; } = TimeSpan.FromMinutes(30);
    }

    public class MongoConfig
    {
        public string? ConnectionString { get; set; }
        public string? DatabaseName { get; set; }
        public string? CollectionName { get; set; }
    }

    public class PostGISConfig
    {
        public string? ConnectionString { get; set; }
        public string? TableName { get; set; } = "locations";
    }

    public class OSRMConfig
    {
        public string? Url { get; set; } = "http://localhost:5000";
        public string? Profile { get; set; } = "driving"; // Ví dụ: driving, walking, cycling
        public string? DataPath { get; set; } // Đường dẫn đến file OSM nếu cần
    }
    public class MapProviderConfig
    {
        public string Provider { get; set; } = "OSRM"; // Mặc định là OSRM
    }

    public class GoogleMapsConfig
    {
        public string Url { get; set; } = "https://maps.googleapis.com/maps/api";
        public string ApiKey { get; set; } = string.Empty;
    }

    public class MapboxConfig
    {
        public string Url { get; set; } = "https://api.mapbox.com";
        public string ApiKey { get; set; } = string.Empty;
    }
}
