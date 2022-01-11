namespace LegacyFighter.Cabs.Entity;

public class CarTypeActiveCounter
{
  private CarType.CarClasses CarClass { get; }

  public int ActiveCarsCounter { get; } = 0;

  public CarTypeActiveCounter(CarType.CarClasses carClass)
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