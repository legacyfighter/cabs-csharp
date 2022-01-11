namespace LegacyFighter.Cabs.Entity;

public class CarTypeActiveCounter
{
  private readonly CarType _carType;

  public CarTypeActiveCounter(CarType carType)
  {
    this._carType = carType;
  }

  public void RegisterActiveCar()
  {
    _carType.RegisterActiveCar();
  }

  public void UnregisterActiveCar()
  {
    _carType.UnregisterActiveCar();
  }

  public int ActiveCarsCounter => _carType.ActiveCarsCounter;
}