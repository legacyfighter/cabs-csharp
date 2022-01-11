using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Entity;
using NodaTime;

namespace LegacyFighter.Cabs.Service;

public class TransactionalDriverTrackingService : IDriverTrackingService
{
  private readonly IDriverTrackingService _inner;
  private readonly ITransactions _transactions;

  public TransactionalDriverTrackingService(IDriverTrackingService inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }
  
  public async Task<DriverPosition> RegisterPosition(long? driverId, double latitude, double longitude)
  {
    await using var tx = await _transactions.BeginTransaction();
    var position = await _inner.RegisterPosition(driverId, latitude, longitude);
    await tx.Commit();
    return position;
  }

  public Task<double> CalculateTravelledDistance(long? driverId, Instant @from, Instant to)
  {
    return _inner.CalculateTravelledDistance(driverId, @from, to);
  }
}