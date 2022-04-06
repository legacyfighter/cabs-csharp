using LegacyFighter.Cabs.Config;

namespace LegacyFighter.Cabs.CarFleet;

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
    var loaded = await Load(id);
    return new CarTypeDto(loaded, (await _carTypeRepository.FindActiveCounter(loaded.CarClass)).ActiveCarsCounter);
  }

  public async Task<CarTypeDto> Create(CarTypeDto carTypeDto)
  {
    var byCarClass = await _carTypeRepository.FindByCarClass(carTypeDto.CarClass);
    if (byCarClass == null)
    {
      var type = new CarType(carTypeDto.CarClass, carTypeDto.Description,
        GetMinNumberOfCars(carTypeDto.CarClass));
      return await LoadDto((await _carTypeRepository.Save(type)).Id);
    }
    else
    {
      byCarClass.Description = carTypeDto.Description;
      return await LoadDto((await _carTypeRepository.FindByCarClass(carTypeDto.CarClass)).Id);
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

  public async Task RegisterCar(CarClasses carClass)
  {
    var carType = await FindByCarClass(carClass);
    carType.RegisterCar();
  }

  public async Task UnregisterCar(CarClasses? carClass)
  {
    var carType = await FindByCarClass(carClass);
    carType.UnregisterCar();
  }

  public async Task UnregisterActiveCar(CarClasses carClass)
  {
    await _carTypeRepository.DecrementCounter(carClass);
  }

  public async Task RegisterActiveCar(CarClasses? carClass)
  {
    await _carTypeRepository.IncrementCounter(carClass.Value);
  }

  public async Task<List<CarClasses>> FindActiveCarClasses()
  {
    return (await _carTypeRepository.FindByStatus(CarType.Statuses.Active))
      .Select(type => type.CarClass)
      .ToList();
  }

  private int GetMinNumberOfCars(CarClasses carClass)
  {
    if (carClass == CarClasses.Eco)
    {
      return _appProperties.MinNoOfCarsForEcoClass;
    }
    else
    {
      return 10;
    }
  }

  public async Task RemoveCarType(CarClasses carClass)
  {
    var carType = await _carTypeRepository.FindByCarClass(carClass);
    if (carType != null)
    {
      await _carTypeRepository.Delete(carType);
    }
  }

  private async Task<CarType> FindByCarClass(CarClasses? carClass)
  {
    var byCarClass = await _carTypeRepository.FindByCarClass(carClass);
    if (byCarClass == null)
    {
      throw new ArgumentException("Car class does not exist: " + carClass);
    }

    return byCarClass;
  }
}