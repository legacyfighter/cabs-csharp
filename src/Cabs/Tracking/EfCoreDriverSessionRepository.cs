using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Tracking;

public interface IDriverSessionRepository
{

  Task<List<DriverSession>> FindAllByLoggedOutAtNullAndDriverIdInAndCarClassIn(ICollection<long?> driverIds,
    List<CarClasses> carClasses);

  Task<DriverSession> FindTopByDriverIdAndLoggedOutAtIsNullOrderByLoggedAtDesc(long? driverId);

  Task<List<DriverSession>> FindByDriverId(long? driverId);
  Task<DriverSession> Save(DriverSession session);
  Task<DriverSession> Find(long sessionId);
}

internal class EfCoreDriverSessionRepository : IDriverSessionRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreDriverSessionRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<List<DriverSession>> FindAllByLoggedOutAtNullAndDriverIdInAndCarClassIn(
    ICollection<long?> driverIds,
    List<CarClasses> carClasses)
  {
    var driverSessions = await _context.DriverSessions.Where(d =>
      d.LoggedOutAt == null && driverIds.Contains(d.DriverId) && carClasses.Cast<CarClasses?>().Contains(d.CarClass))
      .ToListAsync();
    return driverSessions;
  }

  public async Task<DriverSession> FindTopByDriverIdAndLoggedOutAtIsNullOrderByLoggedAtDesc(long? driverId)
  {
    return await _context.DriverSessions
      .Where(d => d.DriverId == driverId && d.LoggedOutAt == null)
      .OrderByDescending(d => d.LoggedOutAt)
      .FirstOrDefaultAsync();
  }

  public async Task<List<DriverSession>> FindByDriverId(long? driverId)
  {
    return await _context.DriverSessions.Where(session => session.DriverId == driverId).ToListAsync();
  }

  public async Task<DriverSession> Save(DriverSession session)
  {
    _context.DriverSessions.Update(session);
    await _context.SaveChangesAsync();
    return session;
  }

  public async Task<DriverSession> Find(long sessionId)
  {
    return await _context.DriverSessions.FindAsync(sessionId);
  }
}