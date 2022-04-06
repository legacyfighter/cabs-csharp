using LegacyFighter.Cabs.Geolocation.Address;
using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.Ride;

[ApiController]
[Route("[controller]")]
public class TransitController
{
  private readonly IRideService _rideService;

  public TransitController(IRideService rideService)
  {
    _rideService = rideService;
  }

  [HttpGet("/transits/{requestGuid}")]
  public async Task<TransitDto> GetTransit(Guid requestGuid)
  {
    return await _rideService.LoadTransit(requestGuid);
  }

  [HttpPost("/transits/")]
  public async Task<TransitDto> CreateTransit([FromBody] TransitDto transitDto)
  {
    return await _rideService.CreateTransit(transitDto);
  }

  [HttpPost("/transits/{id}/changeAddressTo")]
  public async Task<TransitDto> ChangeAddressTo(long? id, [FromBody] AddressDto addressDto)
  {
    await _rideService.ChangeTransitAddressTo(await _rideService.GetRequestGuid(id), addressDto);
    return await _rideService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/changeAddressFrom")]
  public async Task<TransitDto> ChangeAddressFrom(long? id, [FromBody] AddressDto addressDto)
  {
    await _rideService.ChangeTransitAddressFrom(await _rideService.GetRequestGuid(id), addressDto);
    return await _rideService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/cancel")]
  public async Task<TransitDto> Cancel(long? id)
  {
    await _rideService.CancelTransit(await _rideService.GetRequestGuid(id));
    return await _rideService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/publish")]
  public async Task<TransitDto> PublishTransit(long? id)
  {
    await _rideService.PublishTransit(await _rideService.GetRequestGuid(id));
    return await _rideService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/findDrivers")]
  public async Task<TransitDto> FindDriversForTransit(long? id)
  {
    await _rideService.FindDriversForTransit(await _rideService.GetRequestGuid(id));
    return await _rideService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/accept/{driverId}")]
  public async Task<TransitDto> AcceptTransit(long? id,long? driverId)
  {
    await _rideService.AcceptTransit(driverId, await _rideService.GetRequestGuid(id));
    return await _rideService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/start/{driverId}")]
  public async Task<TransitDto> Start(long? id,long? driverId)
  {
    await _rideService.StartTransit(driverId, await _rideService.GetRequestGuid(id));
    return await _rideService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/reject/{driverId}")]
  public async Task<TransitDto> Reject(long? id,long? driverId)
  {
    await _rideService.RejectTransit(driverId, await _rideService.GetRequestGuid(id));
    return await _rideService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/complete/{driverId}")]
  public async Task<TransitDto> Complete(long? id,long? driverId, [FromBody] AddressDto destination)
  {
    await _rideService.CompleteTransit(driverId, await _rideService.GetRequestGuid(id), destination);
    return await _rideService.LoadTransit(id);
  }
}