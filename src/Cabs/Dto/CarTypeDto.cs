using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Dto;

public class CarTypeDto
{
  public CarTypeDto(CarType carType, int activeCarsCounter)
  {
    Id = carType.Id;
    CarClass = carType.CarClass;
    Status = carType.Status;
    CarsCounter = carType.CarsCounter;
    Description = carType.Description;
    ActiveCarsCounter = activeCarsCounter;
    MinNoOfCarsToActivateClass = carType.MinNoOfCarsToActivateClass;
  }

  public CarTypeDto()
  {

  }

  public long? Id { get; }
  public CarType.CarClasses CarClass { get; set; }
  public CarType.Statuses? Status { get; set; }
  public int CarsCounter { get; set; }
  public string Description { get; set; }
  public int ActiveCarsCounter { get; set; }
  public int MinNoOfCarsToActivateClass { get; set; }
}