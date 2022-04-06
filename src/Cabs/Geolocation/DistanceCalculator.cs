using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Geolocation;

public class DistanceCalculator
{
  public double CalculateByMap(double latitudeFrom, double longitudeFrom, double latitudeTo, double longitudeTo)
  {
    // ...

    return 42;
  }

  public double CalculateByGeo(double latitudeFrom, double longitudeFrom, double latitudeTo, double longitudeTo)
  {
    // https://www.geeksforgeeks.org/program-distance-two-points-earth/
    // Using extension method ToRadians which converts from
    // degrees to radians.
    var lon1 = longitudeFrom.ToRadians();
    var lon2 = longitudeTo.ToRadians();
    var lat1 = latitudeFrom.ToRadians();
    var lat2 = latitudeTo.ToRadians();

    // Haversine formula
    var dlon = lon2 - lon1;
    var dlat = lat2 - lat1;
    var a = Math.Pow(Math.Sin(dlat / 2), 2)
            + Math.Cos(lat1) * Math.Cos(lat2)
                             * Math.Pow(Math.Sin(dlon / 2), 2);

    var c = 2 * Math.Asin(Math.Sqrt(a));

    // Radius of earth in kilometers. Use 3956 for miles
    double r = 6371;

    // calculate the result
    var distanceInKMeters = c * r;

    return distanceInKMeters;
  }
}