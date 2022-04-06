using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace LegacyFighter.Cabs.Tracking;

[ApiController]
[Route("[controller]")]
public class DriverTrackingController
{
  private readonly IDriverTrackingService _trackingService;

  public DriverTrackingController(IDriverTrackingService trackingService)
  {
    _trackingService = trackingService;
  }

  [HttpPost("/driverPositions/")]
  public async Task<DriverPositionDto> Create(DriverPositionDto driverPositionDto)
  {
    var driverPosition = await _trackingService.RegisterPosition(
      driverPositionDto.DriverId,
      driverPositionDto.Latitude, 
      driverPositionDto.Longitude,
      driverPositionDto.SeenAt);
    return ToDto(driverPosition);
  }

  [HttpGet("/driverPositions/{id}/total")]
  public async Task<double> CalculateTravelledDistance(long? id, [FromQuery] Instant from, [FromQuery] Instant to)
  {
    return (await _trackingService.CalculateTravelledDistance(id, from, to)).ToKmInDouble();
  }

  private DriverPositionDto ToDto(DriverPosition driverPosition)
  {
    var dto = new DriverPositionDto
    {
      DriverId = driverPosition.DriverId,
      Latitude = driverPosition.Latitude,
      Longitude = driverPosition.Longitude,
      SeenAt = driverPosition.SeenAt
    };
    return dto;
  }
}