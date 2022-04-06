using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Tracking;

public class DriverSession : BaseEntity
{
  internal string CarBrand { get; set; }
  internal Instant LoggedAt { get; set; }
  internal Instant? LoggedOutAt { get; set; }
  internal long? DriverId { get; set; }
  internal string PlatesNumber { get; set; }
  internal CarClasses? CarClass { get; set; }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as DriverSession)?.Id;
  }

  public static bool operator ==(DriverSession left, DriverSession right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(DriverSession left, DriverSession right)
  {
    return !Equals(left, right);
  }
}