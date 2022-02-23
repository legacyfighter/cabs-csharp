using System.Globalization;

namespace LegacyFighter.Cabs.DistanceValue;

public class Distance : IEquatable<Distance>
{
  private const double MilesToKilometersRatio = 1.609344f;

  private readonly double _km;

  public static readonly Distance Zero = OfKm(0);

  public static Distance OfKm(float km)
  {
    return new Distance(km);
  }

  public static Distance OfKm(double km)
  {
    return new Distance(km);
  }

  protected Distance()
  {
  }

  private Distance(double km)
  {
    _km = km;
  }

  public float ToKmInFloat()
  {
    return (float)_km;
  }

  public string PrintIn(string unit)
  {
    var usCulture = CultureInfo.CreateSpecificCulture("en-US");
    if (unit == "km")
    {
      if (_km == Math.Ceiling(_km))
      {
        return Math.Round(_km).ToString(usCulture) + "km";

      }

      return _km.ToString("0.000", usCulture) + "km";
    }

    if (unit == "miles")
    {
      var distance = _km / MilesToKilometersRatio;
      if (distance == Math.Ceiling(distance))
      {
        return Math.Round(distance).ToString(usCulture) + "miles";
      }

      return distance.ToString("0.000", usCulture) + "miles";
    }

    if (unit == "m")
    {
      return Math.Round(_km * 1000).ToString(usCulture) + "m";
    }

    throw new ArgumentException("Invalid unit " + unit);
  }

  public bool Equals(Distance other)
  {
    if (ReferenceEquals(null, other)) return false;
    if (ReferenceEquals(this, other)) return true;
    return _km.Equals(other._km);
  }

  public override bool Equals(object obj)
  {
    return ReferenceEquals(this, obj) || obj is Distance other && Equals(other);
  }

  public override int GetHashCode()
  {
    return _km.GetHashCode();
  }

  public static bool operator ==(Distance left, Distance right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(Distance left, Distance right)
  {
    return !Equals(left, right);
  }

  public double ToKmInDouble() 
  {
    return _km;
  }

  public override string ToString() 
  {
    return "Distance{" +
           "km=" + _km +
           '}';
  }

  public Distance Add(Distance travelled)
  {
    return OfKm(_km + travelled._km);
  }
}