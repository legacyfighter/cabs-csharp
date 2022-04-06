using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace LegacyFighter.Cabs.Tracking;

public interface IDriverPositionRepository
{
  Task<List<DriverPositionDtoV2>> FindAverageDriverPositionSince(double latitudeMin, double latitudeMax,
    double longitudeMin, double longitudeMax, Instant date);

  Task<DriverPosition> Save(DriverPosition position);
}

internal class EfCoreDriverPositionRepository : IDriverPositionRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreDriverPositionRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<List<DriverPositionDtoV2>> FindAverageDriverPositionSince(double latitudeMin,
    double latitudeMax,
    double longitudeMin,
    double longitudeMax,
    Instant date)
  {
    return await (from position in _context.DriverPositions
      where position.Latitude >= latitudeMin && position.Latitude <= latitudeMax
                                             && position.Longitude >= longitudeMin 
                                             && position.Longitude <= longitudeMax
                                             && position.SeenAt >= date
      group position by position.DriverId
      into positionGroup
      select new DriverPositionDtoV2(
        positionGroup.First().DriverId,
        positionGroup.Average(p => p.Latitude),
        positionGroup.Average(p => p.Longitude),
        positionGroup.Max(p => p.SeenAt)
      )).ToListAsync();
  }

  public async Task<DriverPosition> Save(DriverPosition position)
  {
    _context.DriverPositions.Update(position);
    await _context.SaveChangesAsync();
    return position;
  }
}