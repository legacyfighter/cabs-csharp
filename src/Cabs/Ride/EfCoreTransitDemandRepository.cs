using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Ride;

public interface ITransitDemandRepository
{
  Task<TransitDemand> FindByTransitRequestGuid(Guid requestGuid);
  Task Save(TransitDemand transitDemand);
}
  
internal class EfCoreTransitDemandRepository : ITransitDemandRepository
{
  private readonly SqLiteDbContext _dbContext;

  public EfCoreTransitDemandRepository(SqLiteDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<TransitDemand> FindByTransitRequestGuid(Guid requestGuid)
  {
    return (await _dbContext.TransitDemands.FromSqlInterpolated(
      $"SELECT * FROM TransitDemands where TransitRequestGuid = {requestGuid} LIMIT 1").ToListAsync()).FirstOrDefault();
  }

  public async Task Save(TransitDemand transitDemand)
  {
    _dbContext.TransitDemands.Update(transitDemand);
    await _dbContext.SaveChangesAsync();
  }
}