using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Repository;

public interface IDriverAttributeRepository
{
  Task Save(DriverAttribute driverAttribute);
}

internal class EfCoreDriverAttributeRepository : IDriverAttributeRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreDriverAttributeRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task Save(DriverAttribute driverAttribute)
  {
    _context.DriverAttributes.Update(driverAttribute);
    await _context.SaveChangesAsync();
  }
}