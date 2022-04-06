using LegacyFighter.Cabs.Entity;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Repository;

public interface IDriverFeeRepository
{
  Task<DriverFee> FindByDriverId(long? driverId);
  Task<DriverFee> Save(DriverFee driverFee);
}

internal class EfCoreDriverFeeRepository : IDriverFeeRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreDriverFeeRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<DriverFee> FindByDriverId(long? driverId)
  {
    return await _context.DriverFees.FirstOrDefaultAsync(f => f.Driver.Id == driverId);
  }

  public async Task<DriverFee> Save(DriverFee driverFee)
  {
    _context.DriverFees.Update(driverFee);
    await _context.SaveChangesAsync();
    return driverFee;
  }
}