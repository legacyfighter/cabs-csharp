using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Entity.Miles;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Repository;

public interface IAwardedMilesRepository
{
  Task<List<AwardedMiles>> FindAllByClient(Client client);
  Task Save(AwardedMiles miles);
}

internal class EfCoreAwardedMilesRepository : IAwardedMilesRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreAwardedMilesRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public Task<List<AwardedMiles>> FindAllByClient(Client client)
  {
    return _context.AwardedMiles.Where(m => m.Client == client).ToListAsync();
  }

  public async Task Save(AwardedMiles miles)
  {
    _context.AwardedMiles.Update(miles);
    await _context.SaveChangesAsync();
  }
}