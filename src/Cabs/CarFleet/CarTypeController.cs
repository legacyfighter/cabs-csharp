using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.CarFleet;

[ApiController]
[Route("[controller]")]
public class CarTypeController
{
  private readonly ICarTypeService _carTypeService;

  public CarTypeController(ICarTypeService carTypeService)
  {
    _carTypeService = carTypeService;
  }

  [HttpPost("/cartypes")]
  public async Task<CarTypeDto> Create([FromBody] CarTypeDto carTypeDto)
  {
    var created = await _carTypeService.Create(carTypeDto);
    return created;
  }

  [HttpPost("/cartypes/{carClass}/registerCar")]
  public async Task<IActionResult> RegisterCar(CarClasses carClass)
  {
    await _carTypeService.RegisterCar(carClass);
    return new OkResult();
  }

  [HttpPost("/cartypes/{carClass}/unregisterCar")]
  public async Task<IActionResult> UnregisterCar(CarClasses carClass)
  {
    await _carTypeService.UnregisterCar(carClass);
    return new OkResult();
  }

  [HttpPost("/cartypes/{id}/activate")]
  public async Task<IActionResult> Activate(long? id)
  {
    await _carTypeService.Activate(id);
    return new OkResult();
  }

  [HttpPost("/cartypes/{id}/deactivate")]
  public async Task<IActionResult> Deactivate(long? id)
  {
    await _carTypeService.Deactivate(id);
    return new OkResult();
  }

  [HttpGet("/cartypes/{id}")]
  public async Task<CarTypeDto> Find(long? id)
  {
    var carType = await _carTypeService.LoadDto(id);
    return carType;
  }
}