using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace LegacyFighter.Cabs.Tracking;

[ApiController]
[Route("[controller]")]
public class DriverSessionController
{
  private readonly IDriverSessionService _driverSessionService;
  private IClock _clock;

  public DriverSessionController(IDriverSessionService driverSessionService, IClock clock)
  {
    _driverSessionService = driverSessionService;
    _clock = clock;
  }

  [HttpPost("/drivers/{driverId}/driverSessions/login")]
  public async Task<IActionResult> LogIn( long? driverId, [FromBody] DriverSessionDto dto)
  {
    await _driverSessionService.LogIn(driverId, dto.PlatesNumber, dto.CarClass, dto.CarBrand);
    return new OkResult();
  }

  [HttpDelete("/drivers/{driverId}/driverSessions/{sessionId}")]
  public async Task<IActionResult> LogOut( long driverId, long sessionId)
  {
    await _driverSessionService.LogOut(sessionId);
    return new OkResult();
  }

  [HttpDelete("/drivers/{driverId}/driverSessions/")]
  public async Task<IActionResult> LogOutCurrent( long? driverId)
  {
    await _driverSessionService.LogOutCurrentSession(driverId);
    return new OkResult();
  }

  [HttpGet("/drivers/{driverId}/driverSessions/")]
  public async Task<List<DriverSessionDto>> List( long? driverId)
  {
    return (await _driverSessionService.FindByDriver(driverId))
      .Select(session => new DriverSessionDto(session)).ToList();
  }
}