using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Entity;

public class CarType : BaseEntity
{
  public enum Statuses
  {
    Inactive,
    Active
  }

  public enum CarClasses
  {
    Eco,
    Regular,
    Van,
    Premium
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

  [Obsolete]
  public void RegisterActiveCar()
  {
    ActiveCarsCounter++;
  }

  [Obsolete]
  public void UnregisterActiveCar()
  {
    ActiveCarsCounter--;
  }

  public void RegisterCar()
  {
    CarsCounter++;
  }

  public void UnregisterCar()
  {
    CarsCounter--;
    if (CarsCounter < 0)
    {
      throw new InvalidOperationException();
    }
  }

  public void Activate()
  {
    if (CarsCounter < MinNoOfCarsToActivateClass)
    {
      throw new InvalidOperationException("Cannot activate car class when less than " + MinNoOfCarsToActivateClass +
                                      " cars in the fleet");
    }

    Status = Statuses.Active;
  }

  public void Deactivate()
  {
    Status = Statuses.Inactive;
  }

  public CarClasses CarClass { get; set; }
  public string Description { get; set; }
  public Statuses? Status { get; private set; } = Statuses.Inactive;
  public int CarsCounter { get; private set; }
  [Obsolete]
  public int ActiveCarsCounter { get; private set; }
  public int MinNoOfCarsToActivateClass { get; private set; }

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