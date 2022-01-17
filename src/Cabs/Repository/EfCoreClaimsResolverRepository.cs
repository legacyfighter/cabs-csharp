using LegacyFighter.Cabs.Entity;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Repository;

public interface IClaimsResolverRepository
{
  Task<ClaimsResolver> FindByClientId(long? clientId);
  Task<ClaimsResolver> SaveAsync(ClaimsResolver claimsResolver);
}

public class EfCoreClaimsResolverRepository : IClaimsResolverRepository
{
  private readonly SqLiteDbContext _dbContext;

  public EfCoreClaimsResolverRepository(SqLiteDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public Task<ClaimsResolver> FindByClientId(long? clientId)
  {
    return _dbContext.ClaimsResolvers.SingleOrDefaultAsync(resolver => resolver.ClientId == clientId);
  }

  public async Task<ClaimsResolver> SaveAsync(ClaimsResolver claimsResolver)
  {
    _dbContext.Update(claimsResolver);
    await _dbContext.SaveChangesAsync();
    return claimsResolver;
  }
}