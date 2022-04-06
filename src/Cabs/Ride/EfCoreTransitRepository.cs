using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Ride;

public interface ITransitRepository
{
  Task<Transit> FindByTransitRequestGuid(Guid transitRequestGuid);
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

  public async Task<Transit> FindByTransitRequestGuid(Guid transitRequestGuid)
  {
    return await _context.Transits.FirstOrDefaultAsync(
      t => t.RequestGuid == transitRequestGuid);
  }

  public async Task<List<Transit>> FindByClientId(long? clientId)
  {
    return await _context.Transits.Join(
        _context.TransitsDetails,
        transit => transit.RequestGuid,
        details => details.RequestGuid,
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