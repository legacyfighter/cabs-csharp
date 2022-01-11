using System.Globalization;

namespace LegacyFighter.Cabs.DistanceValue;

public sealed class Distance : IEquatable<Distance>
{
  private const float MilesToKilometersRatio = 1.609344f;

  private readonly float _km;

  public static Distance OfKm(float km)
  {
    return new Distance(km);
  }

  private Distance(float km)
  {
    _km = km;
  }

  public float ToKmInFloat()
  {
    return _km;
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
}