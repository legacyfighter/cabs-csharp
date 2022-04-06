using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.Entity;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace LegacyFighter.Cabs.Repository;

public interface ITransitRepository
{
  Task<List<Transit>> FindAllByDriverAndDateTimeBetween(Driver driver, Instant from, Instant to);
  Task<List<Transit>> FindAllByStatus(Transit.Statuses status);
  Task<List<Transit>> FindByClientId(long? clientId);
  Task<Transit> Find(long? transitId);
  Task<Transit> Save(Transit transit);
}

internal class EfCoreTransitRepository : ITransitRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreTransitRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<List<Transit>> FindAllByDriverAndDateTimeBetween(Driver driver, Instant from, Instant to)
  {
    return await _context.Transits.Join(
        _context.TransitsDetails,
        transit => transit.Id,
        details => details.TransitId,
        (transit, details) => new { Transit = transit, Details = details })
      .Where(r => r.Transit.DriverId == driver.Id &&
                  r.Details.DateTime >= from &&
                  r.Details.DateTime <= to)
      .Select(r => r.Transit)
      .ToListAsync();
  }

  public async Task<List<Transit>> FindAllByStatus(Transit.Statuses status)
  {
    return await _context.Transits.Where(t => t.Status == status).ToListAsync();
  }

  public async Task<List<Transit>> FindByClientId(long? clientId)
  {
    return await _context.Transits.Join(
        _context.TransitsDetails,
        transit => transit.Id,
        details => details.TransitId,
        (transit, details) => new { Transit = transit, Details = details })
      .Where(r => r.Details.Client.Id == clientId)
      .Select(r => r.Transit)
      .ToListAsync();
  }

  public async Task<Transit> Find(long? transitId)
  {
    return await _context.Transits.FindAsync(transitId);
  }

  public async Task<Transit> Save(Transit transit)
  {
    _context.Transits.Update(transit);
    await _context.SaveChangesAsync();
    return transit;
  }
}