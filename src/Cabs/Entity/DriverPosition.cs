using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity;

public class DriverPosition : BaseEntity
{
  public DriverPosition()
  {
  }

  public DriverPosition(Driver driver, Instant seenAt, double latitude, double longitude) 
  {
    Driver = driver;
    SeenAt = seenAt;
    Latitude = latitude;
    Longitude = longitude;
  }

  public virtual Driver Driver { set; get; }
  public double Latitude { get; set; }
  public double Longitude { get; set; }
  public Instant SeenAt { get; set; }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as DriverPosition)?.Id;
  }

  public static bool operator ==(DriverPosition left, DriverPosition right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(DriverPosition left, DriverPosition right)
  {
    return !Equals(left, right);
  }
}