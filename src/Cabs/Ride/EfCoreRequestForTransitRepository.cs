using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Ride;

public interface IRequestForTransitRepository
{
  public Task<RequestForTransit> FindByRequestGuid(Guid requestGuid);
  Task<RequestForTransit> Save(RequestForTransit requestForTransit);
  Task<RequestForTransit> Find(long? requestId);
}

public class EfCoreRequestForTransitRepository : IRequestForTransitRepository
{
  private readonly SqLiteDbContext _dbContext;

  public EfCoreRequestForTransitRepository(SqLiteDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<RequestForTransit> FindByRequestGuid(Guid requestGuid)
  {
    return await _dbContext.RequestsForTransit
      .FirstOrDefaultAsync(r => r.RequestGuid == requestGuid);
  }

  public async Task<RequestForTransit> Save(RequestForTransit requestForTransit)
  {
    _dbContext.RequestsForTransit.Update(requestForTransit);
    await _dbContext.SaveChangesAsync();
    return requestForTransit;
  }

  public async Task<RequestForTransit> Find(long? requestId)
  {
    return await _dbContext.RequestsForTransit.FindAsync(requestId);
  }
}