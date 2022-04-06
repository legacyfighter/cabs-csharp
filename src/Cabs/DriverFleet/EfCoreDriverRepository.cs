using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.DriverFleet;

public interface IDriverRepository
{
  Task<Driver> Find(long? driverId);
  Task<Driver> Save(Driver driver);
}

internal class EfCoreDriverRepository : IDriverRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreDriverRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<Driver> Find(long? driverId)
  {
    return await _context.Drivers.FindAsync(driverId);
  }

  public async Task<Driver> Save(Driver driver)
  {
    _context.Drivers.Update(driver);
    await _context.SaveChangesAsync();
    return driver;
  }
}