using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodDeliverySystem.Common.Geo.Models
{
    public class OsrmRouteResponse
    {
        public string Code { get; set; }
        public string Message { get; set; } // optional
        public List<Route> Routes { get; set; }
        public List<Waypoint> Waypoints { get; set; }

        public class Route
        {
            public List<Leg> Legs { get; set; }
            public string WeightName { get; set; }
            public double Weight { get; set; }
            public double Duration { get; set; }
            public double Distance { get; set; }
        }

        public class Leg
        {
            public List<object> Steps { get; set; } // steps là mảng rỗng, không cần định nghĩa cụ thể
            public double Weight { get; set; }
            public string Summary { get; set; }
            public double Duration { get; set; }
            public double Distance { get; set; }
        }

        public class Waypoint
        {
            public string Hint { get; set; }
            public List<double> Location { get; set; } // [lon, lat]
            public string Name { get; set; }
            public double Distance { get; set; }
        }
    }
}
