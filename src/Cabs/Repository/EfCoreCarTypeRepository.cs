using LegacyFighter.Cabs.Entity;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Repository;

public interface ICarTypeRepository
{
  Task<CarType> FindByCarClass(CarType.CarClasses? carClass);
  Task<List<CarType>> FindByStatus(CarType.Statuses status);
  Task<CarType> Find(long? id);
  Task<CarType> Save(CarType type);
  Task Delete(CarType carType);
}

internal class EfCoreCarTypeRepository : ICarTypeRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreCarTypeRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<CarType> FindByCarClass(CarType.CarClasses? carClass)
  {
    return await _context.CarTypes.FirstOrDefaultAsync(c => c.CarClass == carClass);
  }

  public async Task<List<CarType>> FindByStatus(CarType.Statuses status)
  {
    return await _context.CarTypes.Where(t => t.Status == status).ToListAsync();
  }

  public async Task<CarType> Find(long? id)
  {
    return await _context.CarTypes.FindAsync(id);
  }

  public async Task<CarType> Save(CarType type)
  {
    _context.CarTypes.Update(type);
    await _context.SaveChangesAsync();
    return type;
  }

  public async Task Delete(CarType carType)
  {
    _context.CarTypes.Remove(carType);
    await _context.SaveChangesAsync();
  }
}