using LegacyFighter.Cabs.Geolocation.Address;
using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.Ride;

[ApiController]
[Route("[controller]")]
public class TransitController
{
  private readonly ITransitService _transitService;

  public TransitController(ITransitService transitService)
  {
    _transitService = transitService;
  }

  [HttpGet("/transits/{requestGuid}")]
  public async Task<TransitDto> GetTransit(Guid requestGuid)
  {
    return await _transitService.LoadTransit(requestGuid);
  }

  [HttpPost("/transits/")]
  public async Task<TransitDto> CreateTransit([FromBody] TransitDto transitDto)
  {
    return await _transitService.CreateTransit(transitDto);
  }

  [HttpPost("/transits/{id}/changeAddressTo")]
  public async Task<TransitDto> ChangeAddressTo(long? id, [FromBody] AddressDto addressDto)
  {
    await _transitService.ChangeTransitAddressTo(await _transitService.GetRequestGuid(id), addressDto);
    return await _transitService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/changeAddressFrom")]
  public async Task<TransitDto> ChangeAddressFrom(long? id, [FromBody] AddressDto addressDto)
  {
    await _transitService.ChangeTransitAddressFrom(await _transitService.GetRequestGuid(id), addressDto);
    return await _transitService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/cancel")]
  public async Task<TransitDto> Cancel(long? id)
  {
    await _transitService.CancelTransit(await _transitService.GetRequestGuid(id));
    return await _transitService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/publish")]
  public async Task<TransitDto> PublishTransit(long? id)
  {
    await _transitService.PublishTransit(await _transitService.GetRequestGuid(id));
    return await _transitService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/findDrivers")]
  public async Task<TransitDto> FindDriversForTransit(long? id)
  {
    await _transitService.FindDriversForTransit(await _transitService.GetRequestGuid(id));
    return await _transitService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/accept/{driverId}")]
  public async Task<TransitDto> AcceptTransit(long? id,long? driverId)
  {
    await _transitService.AcceptTransit(driverId, await _transitService.GetRequestGuid(id));
    return await _transitService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/start/{driverId}")]
  public async Task<TransitDto> Start(long? id,long? driverId)
  {
    await _transitService.StartTransit(driverId, await _transitService.GetRequestGuid(id));
    return await _transitService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/reject/{driverId}")]
  public async Task<TransitDto> Reject(long? id,long? driverId)
  {
    await _transitService.RejectTransit(driverId, await _transitService.GetRequestGuid(id));
    return await _transitService.LoadTransit(id);
  }

  [HttpPost("/transits/{id}/complete/{driverId}")]
  public async Task<TransitDto> Complete(long? id,long? driverId, [FromBody] AddressDto destination)
  {
    await _transitService.CompleteTransit(driverId, await _transitService.GetRequestGuid(id), destination);
    return await _transitService.LoadTransit(id);
  }
}