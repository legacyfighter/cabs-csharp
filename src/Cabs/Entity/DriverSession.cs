using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity;

public class DriverSession : BaseEntity
{
  public string CarBrand { get; set; }
  public Instant LoggedAt { get; set; }
  public Instant? LoggedOutAt { get; set; }
  public virtual Driver Driver { get; set; }
  public string PlatesNumber { get; set; }
  public CarClasses? CarClass { get; set; }

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