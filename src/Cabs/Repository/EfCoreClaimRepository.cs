using LegacyFighter.Cabs.Entity;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Repository;

public interface IClaimRepository
{
  Task<List<Claim>> FindByOwner(Client owner);

  Task<List<Claim>> FindByOwnerAndTransit(Client owner, Transit transit);

  Task<long> Count();
  Task<Claim> Find(long? id);
  Task<Claim> Save(Claim claim);
}

internal class EfCoreClaimRepository : IClaimRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreClaimRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<List<Claim>> FindByOwner(Client owner)
  {
    return await _context.Claims.Where(c => c.Owner == owner).ToListAsync();
  }

  public async Task<List<Claim>> FindByOwnerAndTransit(Client owner, Transit transit)
  {
    return await _context.Claims.Where(c => c.Owner == owner && c.Transit == transit).ToListAsync();
  }

  public async Task<long> Count()
  {
    return await _context.Claims.CountAsync();
  }

  public async Task<Claim> Find(long? id)
  {
    return await _context.Claims.FindAsync(id);
  }

  public async Task<Claim> Save(Claim claim)
  {
    _context.Claims.Update(claim);
    await _context.SaveChangesAsync();
    return claim;
  }
}