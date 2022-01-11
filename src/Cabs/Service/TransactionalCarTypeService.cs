using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public class TransactionalCarTypeService : ICarTypeService
{
  private readonly ICarTypeService _inner;
  private readonly ITransactions _transactions;

  public TransactionalCarTypeService(
    ICarTypeService inner, 
    ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<CarType> Load(long? id)
  {
    await using var tx = await _transactions.BeginTransaction();
    var carType = await _inner.Load(id);
    await tx.Commit();
    return carType;
  }

  public async Task<CarTypeDto> LoadDto(long? id)
  {
    await using var tx = await _transactions.BeginTransaction();
    var carTypeDto = await _inner.LoadDto(id);
    await tx.Commit();
    return carTypeDto;
  }

  public async Task<CarType> Create(CarTypeDto carTypeDto)
  {
    await using var tx = await _transactions.BeginTransaction();
    var carType = await _inner.Create(carTypeDto);
    await tx.Commit();
    return carType;
  }

  public async Task Activate(long? id)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.Activate(id);
    await tx.Commit();
  }

  public async Task Deactivate(long? id)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.Deactivate(id);
    await tx.Commit();
  }

  public async Task RegisterCar(CarType.CarClasses carClass)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.RegisterCar(carClass);
    await tx.Commit();
  }

  public async Task UnregisterCar(CarType.CarClasses? carClass)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.UnregisterCar(carClass);
    await tx.Commit();
  }
 
  public async Task UnregisterActiveCar(CarType.CarClasses carClass)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.UnregisterActiveCar(carClass);
    await tx.Commit();
  }

  public async Task RegisterActiveCar(CarType.CarClasses? carClass)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.RegisterActiveCar(carClass);
    await tx.Commit();
  }

  public async Task<List<CarType.CarClasses>> FindActiveCarClasses()
  {
    await using var tx = await _transactions.BeginTransaction();
    var activeCarClasses = await _inner.FindActiveCarClasses();
    await tx.Commit();
    return activeCarClasses;
  }

  public async Task RemoveCarType(CarType.CarClasses carClass)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.RemoveCarType(carClass);
    await tx.Commit();
  }
}