using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.CabsTests.Common;

namespace LegacyFighter.CabsTests.Integration;

public class CarTypeUpdateIntegrationTest
{
  private CabsApp _app = default!;
  private ICarTypeService CarTypeService => _app.CarTypeService;

  [SetUp]
  public void InitializeApp()
  {
    _app = CabsApp.CreateInstance();
  }

  [TearDown]
  public async Task DisposeOfApp()
  {
    await _app.DisposeAsync();
  }

  [Test]
  public async Task CanCreateCarType()
  {
    //given
    await ThereIsNoCarClassInTheSystem(CarClasses.Van);

    //when
    var created = await CreateCarClass("duże i dobre", CarClasses.Van);

    //then
    var loaded = await Load(created.Id);
    Assert.AreEqual(CarClasses.Van, loaded.CarClass);
    Assert.AreEqual(0, loaded.CarsCounter);
    Assert.AreEqual(0, loaded.ActiveCarsCounter);
    Assert.AreEqual("duże i dobre", loaded.Description);
  }

  [Test]
  public async Task CanChangeCarDescription()
  {
    //given
    await ThereIsNoCarClassInTheSystem(CarClasses.Van);
    //and
    await CreateCarClass("duże i dobre", CarClasses.Van);

    //when
    var changed = await CreateCarClass("duże i bardzo dobre", CarClasses.Van);

    //then
    var loaded = await Load(changed.Id);
    Assert.AreEqual(CarClasses.Van, loaded.CarClass);
    Assert.AreEqual(0, loaded.CarsCounter);
    Assert.AreEqual("duże i bardzo dobre", loaded.Description);
  }

  [Test]
  public async Task CanRegisterActiveCars()
  {
    //given
    var created = await CreateCarClass("duże i dobre", CarClasses.Van);
    //and
    var currentActiveCarsCount = (await Load(created.Id)).ActiveCarsCounter;

    //when
    await RegisterActiveCar(CarClasses.Van);

    //then
    var loaded = await Load(created.Id);
    Assert.AreEqual(currentActiveCarsCount + 1, loaded.ActiveCarsCounter);
  }

  [Test]
  public async Task CanUnregisterActiveCars()
  {
    //given
    var created = await CreateCarClass("duże i dobre", CarClasses.Van);
    //and
    await RegisterActiveCar(CarClasses.Van);
    //and
    var currentActiveCarsCount = (await Load(created.Id)).ActiveCarsCounter;

    //when
    await UnregisterActiveCar(CarClasses.Van);

    //then
    var loaded = await Load(created.Id);
    Assert.AreEqual(currentActiveCarsCount - 1, loaded.ActiveCarsCounter);
  }

  private async Task RegisterActiveCar(CarClasses carClass)
  {
    await CarTypeService.RegisterActiveCar(carClass);
  }

  private async Task UnregisterActiveCar(CarClasses carClass)
  {
    await CarTypeService.UnregisterActiveCar(carClass);
  }

  private async Task<CarTypeDto> Load(long? id)
  {
    return await CarTypeService.LoadDto(id);
  }

  private async Task<CarTypeDto> CreateCarClass(string desc, CarClasses carClass)
  {
    var carTypeDto = new CarTypeDto
    {
      CarClass = carClass,
      Description = desc
    };
    var carType = await CarTypeService.Create(carTypeDto);
    return await CarTypeService.LoadDto(carType.Id);
  }

  private async Task ThereIsNoCarClassInTheSystem(CarClasses carClass)
  {
    await CarTypeService.RemoveCarType(carClass);
  }
}