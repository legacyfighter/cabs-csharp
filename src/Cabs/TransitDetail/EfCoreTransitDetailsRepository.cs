using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.TransitDetail;

public interface ITransitDetailsRepository
{
  Task<TransitDetails> FindByTransitId(long? transitId);
  Task Save(TransitDetails transitDetails);
}

public class EfCoreTransitDetailsRepository : ITransitDetailsRepository
{
  private readonly SqLiteDbContext _dbContext;

  public EfCoreTransitDetailsRepository(SqLiteDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<TransitDetails> FindByTransitId(long? transitId)
  {
    return await _dbContext.TransitsDetails.FindAsync(transitId);
  }

  public async Task Save(TransitDetails transitDetails)
  {
    _dbContext.TransitsDetails.Update(transitDetails);
    await _dbContext.SaveChangesAsync();
  }
}