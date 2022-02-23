using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace LegacyFighter.Cabs.Repository;

public interface IDriverPositionRepository
{
  Task<List<DriverPositionDtoV2>> FindAverageDriverPositionSince(double latitudeMin, double latitudeMax,
    double longitudeMin, double longitudeMax, Instant date);

  Task<List<DriverPosition>> FindByDriverAndSeenAtBetweenOrderBySeenAtAsc(Driver driver, Instant from, Instant to);
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
      group position by position.Driver.Id
      into positionGroup
      select new DriverPositionDtoV2(
        positionGroup.First().Driver,
        positionGroup.Average(p => p.Latitude),
        positionGroup.Average(p => p.Longitude),
        positionGroup.Max(p => p.SeenAt)
      )).ToListAsync();
  }

  public async Task<List<DriverPosition>> FindByDriverAndSeenAtBetweenOrderBySeenAtAsc(Driver driver, Instant from,
    Instant to)
  {
    return await _context.DriverPositions.Where(
        d => d.Driver == driver && 
             d.SeenAt >= from && 
             d.SeenAt <= to)
      .ToListAsync();
  }

  public async Task<DriverPosition> Save(DriverPosition position)
  {
    _context.DriverPositions.Update(position);
    await _context.SaveChangesAsync();
    return position;
  }
}