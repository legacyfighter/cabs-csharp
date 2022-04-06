using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace LegacyFighter.Cabs.Ride.Details;

public interface ITransitDetailsRepository
{
  Task<TransitDetails> FindByRequestGuid(Guid requestId);
  Task<List<TransitDetails>> FindByClientId(long? clientId);
  Task<List<TransitDetails>> FindAllByDriverAndDateTimeBetween(long? driverId, Instant from, Instant to);
  Task Save(TransitDetails transitDetails);
  Task<List<TransitDetails>> FindByStatus(Statuses status);
  Task<TransitDetails> FindByTransitId(long? transitId);
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

  public async Task<List<TransitDetails>> FindByStatus(Statuses status)
  {
    return await _dbContext.TransitsDetails.Where(td => td.Status == status).ToListAsync();
  }

  public async Task<TransitDetails> FindByRequestGuid(Guid requestId)
  {
    return await _dbContext.TransitsDetails.FirstOrDefaultAsync(
      td => td.RequestGuid == requestId);
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