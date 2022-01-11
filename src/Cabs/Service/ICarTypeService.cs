using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public interface ICarTypeService
{
  Task<CarType> Load(long? id);
  Task<CarTypeDto> LoadDto(long? id);
  Task<CarType> Create(CarTypeDto carTypeDto);
  Task Activate(long? id);
  Task Deactivate(long? id);
  Task RegisterCar(CarType.CarClasses carClass);
  Task UnregisterCar(CarType.CarClasses? carClass);
  Task UnregisterActiveCar(CarType.CarClasses carClass);
  Task RegisterActiveCar(CarType.CarClasses? carClass);
  Task<List<CarType.CarClasses>> FindActiveCarClasses();
  Task RemoveCarType(CarType.CarClasses carClass);
}