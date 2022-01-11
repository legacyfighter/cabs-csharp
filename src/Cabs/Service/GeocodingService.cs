using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public interface IGeocodingService
{
  double[] GeocodeAddress(Address address);
}

public class GeocodingService : IGeocodingService
{
  public double[] GeocodeAddress(Address address)
  {
    //TODO ... call do zewnętrznego serwisu

    var geocoded = new double[2];

    geocoded[0] = 1f; //latitude
    geocoded[1] = 1f; //longitude

    return geocoded;
  }
}