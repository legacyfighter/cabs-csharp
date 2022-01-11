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
  Task<CarTypeActiveCounter> FindActiveCounter(CarType.CarClasses? carClass);
  Task IncrementCounter(CarType.CarClasses carClass);
  Task DecrementCounter(CarType.CarClasses carClass);
}

internal class CarTypeRepository : ICarTypeRepository
{
  private readonly ICarTypeEntityRepository _carTypeEntityRepository;
  private readonly ICarTypeActiveCounterRepository _carTypeActiveCounterRepository;

  public CarTypeRepository(
    ICarTypeEntityRepository carTypeEntityRepository,
    ICarTypeActiveCounterRepository carTypeActiveCounterRepository)
  {
    _carTypeEntityRepository = carTypeEntityRepository;
    _carTypeActiveCounterRepository = carTypeActiveCounterRepository;
  }

  public Task<CarType> FindByCarClass(CarType.CarClasses? carClass)
  {
    return _carTypeEntityRepository.FindByCarClass(carClass);
  }

  public Task<CarTypeActiveCounter> FindActiveCounter(CarType.CarClasses? carClass)
  {
    return _carTypeActiveCounterRepository.FindByCarClass(carClass);
  }

  public Task<List<CarType>> FindByStatus(CarType.Statuses status)
  {
    return _carTypeEntityRepository.FindByStatus(status);
  }

  public async Task<CarType> Save(CarType carType)
  {
    await _carTypeActiveCounterRepository.Save(new CarTypeActiveCounter(carType.CarClass));
    return await _carTypeEntityRepository.Save(carType);
  }

  public async Task<CarType> Find(long? id)
  {
    return await _carTypeEntityRepository.Find(id);
  }

  public async Task Delete(CarType carType)
  {
    await _carTypeEntityRepository.Delete(carType);
    await _carTypeActiveCounterRepository.Delete(
      await _carTypeActiveCounterRepository.FindByCarClass(carType.CarClass));
  }

  public async Task IncrementCounter(CarType.CarClasses carClass)
  {
    await _carTypeActiveCounterRepository.IncrementCounter(carClass);
  }

  public async Task DecrementCounter(CarType.CarClasses carClass)
  {
    await _carTypeActiveCounterRepository.DecrementCounter(carClass);
  }
}

public interface ICarTypeEntityRepository
{
  Task<CarType> FindByCarClass(CarType.CarClasses? carClass);
  Task<List<CarType>> FindByStatus(CarType.Statuses status);
  Task<CarType> Find(long? id);
  Task<CarType> Save(CarType type);
  Task Delete(CarType carType);
}

internal class EfCoreCarTypeRepository : ICarTypeEntityRepository
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

public interface ICarTypeActiveCounterRepository
{
  Task<CarTypeActiveCounter> FindByCarClass(CarType.CarClasses? carTypeCarClass);
  Task Delete(CarTypeActiveCounter item);
  Task<CarTypeActiveCounter> Save(CarTypeActiveCounter carTypeActiveCounter);
  Task DecrementCounter(CarType.CarClasses carClass);
  Task IncrementCounter(CarType.CarClasses carClass);
}

internal class EfCoreCarTypeActiveCounterRepository : ICarTypeActiveCounterRepository
{
  private readonly SqLiteDbContext _dbContext;

  public EfCoreCarTypeActiveCounterRepository(SqLiteDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<CarTypeActiveCounter> FindByCarClass(CarType.CarClasses? carTypeCarClass)
  {
    return await _dbContext.CarTypeActiveCounters.FindAsync(carTypeCarClass);
  }

  public async Task Delete(CarTypeActiveCounter item)
  {
    _dbContext.CarTypeActiveCounters.Remove(item);
    await _dbContext.SaveChangesAsync();
  }

  public async Task<CarTypeActiveCounter> Save(CarTypeActiveCounter carTypeActiveCounter)
  {
    if (!_dbContext.CarTypeActiveCounters.Contains(carTypeActiveCounter))
    {
      _dbContext.CarTypeActiveCounters.Add(carTypeActiveCounter);
    }
    else
    {
      _dbContext.CarTypeActiveCounters.Update(carTypeActiveCounter);
    }

    await _dbContext.SaveChangesAsync();
    return carTypeActiveCounter;
  }

  public async Task DecrementCounter(CarType.CarClasses carClass)
  {
    const string commandText = 
      $"UPDATE {nameof(CarTypeActiveCounter)}s " +
      $"SET {nameof(CarTypeActiveCounter.ActiveCarsCounter)} = {nameof(CarTypeActiveCounter.ActiveCarsCounter)} - 1 " +
      "WHERE CarClass = {0}";
    await _dbContext.Database.ExecuteSqlRawAsync(commandText, Enum.GetName(carClass));
  }

  public async Task IncrementCounter(CarType.CarClasses carClass)
  {
    const string commandText = 
      $"UPDATE {nameof(CarTypeActiveCounter)}s " +
      $"SET {nameof(CarTypeActiveCounter.ActiveCarsCounter)} = {nameof(CarTypeActiveCounter.ActiveCarsCounter)} + 1 " +
      "WHERE CarClass = {0}";
    await _dbContext.Database.ExecuteSqlRawAsync(commandText, Enum.GetName(carClass));
  }
}