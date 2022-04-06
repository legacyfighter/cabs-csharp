using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.CarFleet;

public class CarType : BaseEntity
{
  public enum Statuses
  {
    Inactive,
    Active
  }

  public CarType(CarClasses carClass, string description, int minNoOfCarsToActivateClass)
  {
    CarClass = carClass;
    Description = description;
    MinNoOfCarsToActivateClass = minNoOfCarsToActivateClass;
  }

  protected CarType()
  {
  }

  internal void RegisterCar()
  {
    CarsCounter++;
  }

  internal void UnregisterCar()
  {
    CarsCounter--;
    if (CarsCounter < 0)
    {
      throw new InvalidOperationException();
    }
  }

  internal void Activate()
  {
    if (CarsCounter < MinNoOfCarsToActivateClass)
    {
      throw new InvalidOperationException("Cannot activate car class when less than " + MinNoOfCarsToActivateClass +
                                      " cars in the fleet");
    }

    Status = Statuses.Active;
  }

  internal void Deactivate()
  {
    Status = Statuses.Inactive;
  }

  internal CarClasses CarClass { get; set; }
  internal string Description { get; set; }
  internal Statuses? Status { get; private set; } = Statuses.Inactive;
  internal int CarsCounter { get; private set; }
  internal int MinNoOfCarsToActivateClass { get; private set; }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as CarType)?.Id;
  }

  public static bool operator ==(CarType left, CarType right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(CarType left, CarType right)
  {
    return !Equals(left, right);
  }
}