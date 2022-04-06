using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace LegacyFighter.Cabs.TransitDetail;

public interface ITransitDetailsRepository
{
  Task<TransitDetails> FindByTransitId(long? transitId);
  Task<List<TransitDetails>> FindByClientId(long? clientId);
  Task<List<TransitDetails>> FindAllByDriverAndDateTimeBetween(long? driverId, Instant from, Instant to);
  Task Save(TransitDetails transitDetails);
  Task<List<TransitDetails>> FindByStatus(Transit.Statuses status);
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

  public async Task<List<TransitDetails>> FindByClientId(long? clientId)
  {
    return await _dbContext.TransitsDetails.Where(td => td.Client.Id == clientId).ToListAsync();
  }

  public async Task<List<TransitDetails>> FindByStatus(Transit.Statuses status)
  {
    return await _dbContext.TransitsDetails.Where(td => td.Status == status).ToListAsync();
  }

  public async Task<List<TransitDetails>> FindAllByDriverAndDateTimeBetween(long? driverId, Instant from, Instant to)
  {
    return await _dbContext.TransitsDetails.Where(td => 
      td.DriverId == driverId && 
      td.DateTime >= from &&
      td.DateTime <= to).ToListAsync();
  }

  public async Task Save(TransitDetails transitDetails)
  {
    _dbContext.TransitsDetails.Update(transitDetails);
    await _dbContext.SaveChangesAsync();
  }
}