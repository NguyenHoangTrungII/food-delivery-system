namespace FoodDeliverySystem.Common.Geo.Common;

// NEW: Helper để kiểm tra tọa độ
public static class GeoValidator
{
    public static void ValidateCoordinates(double lat1, double lon1, double lat2, double lon2)
    {
        if (double.IsNaN(lat1) || double.IsNaN(lon1) || lat1 < -90 || lat1 > 90 || lon1 < -180 || lon1 > 180 ||
            double.IsNaN(lat2) || double.IsNaN(lon2) || lat2 < -90 || lat2 > 90 || lon2 < -180 || lon2 > 180)
        {
            throw new ArgumentException("Invalid coordinates.");
        }
        if (lat1 == lat2 && lon1 == lon2)
        {
            throw new ArgumentException("Start and end points are identical.");
        }
    }

    public static void ValidateCoordinates(double lat, double lon)
    {
        if (double.IsNaN(lat) || double.IsNaN(lon) || lat < -90 || lat > 90 || lon < -180 || lon > 180)
        {
            throw new ArgumentException("Invalid coordinates.");
        }
    }

    public static Boolean IsValidCoordinate(double lat, double lon)
    {
        if (double.IsNaN(lat) || double.IsNaN(lon) || lat < -90 || lat > 90 || lon < -180 || lon > 180 )
        {
            return false;
        }
        if (lat == lon && lon == lat)
        {
            return false;
        }

        return true;
    }
}