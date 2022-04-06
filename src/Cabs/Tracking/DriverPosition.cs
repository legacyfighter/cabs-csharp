using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Tracking;

public class DriverPosition : BaseEntity
{
  internal DriverPosition()
  {
  }

  internal long? DriverId { set; get; }
  internal double Latitude { get; set; }
  internal double Longitude { get; set; }
  internal Instant SeenAt { get; set; }

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