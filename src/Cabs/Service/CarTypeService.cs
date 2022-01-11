using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.Service;

public class CarTypeService : ICarTypeService
{
  private readonly ICarTypeRepository _carTypeRepository;
  private readonly IAppProperties _appProperties;

  public CarTypeService(ICarTypeRepository carTypeRepository, IAppProperties appProperties)
  {
    _carTypeRepository = carTypeRepository;
    _appProperties = appProperties;
  }

  public async Task<CarType> Load(long? id)
  {
    var carType = await _carTypeRepository.Find(id);
    if (carType == null)
    {
      throw new InvalidOperationException("Cannot find car type");
    }

    return carType;
  }

  public async Task<CarTypeDto> LoadDto(long? id)
  {
    return new CarTypeDto(await Load(id));
  }

  public async Task<CarType> Create(CarTypeDto carTypeDto)
  {
    var byCarClass = await _carTypeRepository.FindByCarClass(carTypeDto.CarClass);
    if (byCarClass == null)
    {
      var type = new CarType(carTypeDto.CarClass, carTypeDto.Description,
        GetMinNumberOfCars(carTypeDto.CarClass));
      return await _carTypeRepository.Save(type);
    }
    else
    {
      byCarClass.Description = carTypeDto.Description;
      return byCarClass;
    }
  }

  public async Task Activate(long? id)
  {
    var carType = await Load(id);
    carType.Activate();
  }

  public async Task Deactivate(long? id)
  {
    var carType = await Load(id);
    carType.Deactivate();
  }

  public async Task RegisterCar(CarType.CarClasses carClass)
  {
    var carType = await FindByCarClass(carClass);
    carType.RegisterCar();
  }

  public async Task UnregisterCar(CarType.CarClasses? carClass)
  {
    var carType = await FindByCarClass(carClass);
    carType.UnregisterCar();
  }

  public async Task UnregisterActiveCar(CarType.CarClasses carClass)
  {
    var carType = await FindByCarClass(carClass);
    carType.UnregisterActiveCar();
  }

  public async Task RegisterActiveCar(CarType.CarClasses? carClass)
  {
    var carType = await FindByCarClass(carClass);
    carType.RegisterActiveCar();
  }

  public async Task<List<CarType.CarClasses>> FindActiveCarClasses()
  {
    return (await _carTypeRepository.FindByStatus(CarType.Statuses.Active))
      .Select(type => type.CarClass)
      .ToList();
  }

  private int GetMinNumberOfCars(CarType.CarClasses carClass)
  {
    if (carClass == CarType.CarClasses.Eco)
    {
      return _appProperties.MinNoOfCarsForEcoClass;
    }
    else
    {
      return 10;
    }
  }

  public async Task RemoveCarType(CarType.CarClasses carClass)
  {
    var carType = await _carTypeRepository.FindByCarClass(carClass);
    if (carType != null)
    {
      await _carTypeRepository.Delete(carType);
    }
  }

  private async Task<CarType> FindByCarClass(CarType.CarClasses? carClass)
  {
    var byCarClass = await _carTypeRepository.FindByCarClass(carClass);
    if (byCarClass == null)
    {
      throw new ArgumentException("Car class does not exist: " + carClass);
    }

    return byCarClass;
  }
}