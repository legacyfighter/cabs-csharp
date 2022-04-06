namespace LegacyFighter.Cabs.CarFleet;

public interface ICarTypeService
{
  Task<CarType> Load(long? id);
  Task<CarTypeDto> LoadDto(long? id);
  Task<CarTypeDto> Create(CarTypeDto carTypeDto);
  Task Activate(long? id);
  Task Deactivate(long? id);
  Task RegisterCar(CarClasses carClass);
  Task UnregisterCar(CarClasses? carClass);
  Task UnregisterActiveCar(CarClasses carClass);
  Task RegisterActiveCar(CarClasses? carClass);
  Task<List<CarClasses>> FindActiveCarClasses();
  Task RemoveCarType(CarClasses carClass);
}