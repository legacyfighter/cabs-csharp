namespace LegacyFighter.Cabs.CarFleet;

public class CarTypeActiveCounter
{
  private CarClasses CarClass { get; }

  internal int ActiveCarsCounter { get; } = 0;

  public CarTypeActiveCounter(CarClasses carClass)
  {
    CarClass = carClass;
  }

  public CarTypeActiveCounter()
  {
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && CarClass != null && CarClass == (obj as CarType)?.CarClass;
  }

  public static bool operator ==(CarTypeActiveCounter left, CarTypeActiveCounter right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(CarTypeActiveCounter left, CarTypeActiveCounter right)
  {
    return !Equals(left, right);
  }

  public override int GetHashCode()
  {
    return GetType().GetHashCode();
  }
}