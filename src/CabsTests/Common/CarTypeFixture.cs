using System.Linq;
using LegacyFighter.Cabs.CarFleet;

namespace LegacyFighter.CabsTests.Common;

public class CarTypeFixture
{
  private readonly ICarTypeService _carTypeService;

  public CarTypeFixture(ICarTypeService carTypeService)
  {
    _carTypeService = carTypeService;
  }

  public async Task<CarTypeDto> AnActiveCarCategory(CarClasses carClass)
  {
    var carTypeDto = new CarTypeDto
    {
      CarClass = carClass,
      Description = "opis"
    };
    var carType = await _carTypeService.Create(carTypeDto);
    foreach (var _ in Enumerable.Range(1, carType.MinNoOfCarsToActivateClass))
    {
      await _carTypeService.RegisterCar(carType.CarClass);
    }

    await _carTypeService.Activate(carType.Id);
    return carType;
  }
}