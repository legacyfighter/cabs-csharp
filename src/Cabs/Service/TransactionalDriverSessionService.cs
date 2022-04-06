using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public class TransactionalDriverSessionService : IDriverSessionService
{
  private readonly IDriverSessionService _inner;
  private readonly ITransactions _transactions;

  public TransactionalDriverSessionService(IDriverSessionService inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public Task<DriverSession> LogIn(long? driverId, string plateNumber, CarClasses? carClass, string carBrand)
  {
    return _inner.LogIn(driverId, plateNumber, carClass, carBrand);
  }

  public async Task LogOut(long sessionId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.LogOut(sessionId);
    await tx.Commit();
  }

  public async Task LogOutCurrentSession(long? driverId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.LogOutCurrentSession(driverId);
    await tx.Commit();
  }

  public Task<List<DriverSession>> FindByDriver(long? driverId)
  {
    return _inner.FindByDriver(driverId);
  }
}