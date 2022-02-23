using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.Entity;
using NodaTime;

namespace LegacyFighter.Cabs.DriverReports.TravelledDistances;

public class TransactionalTravelledDistanceService : ITravelledDistanceService
{
  private readonly TravelledDistanceService _next;
  private readonly ITransactions _transactions;

  public TransactionalTravelledDistanceService(TravelledDistanceService next, ITransactions transactions)
  {
    _next = next;
    _transactions = transactions;
  }

  public async Task<Distance> CalculateDistance(long driverId, Instant from, Instant to)
  {
    return await _next.CalculateDistance(driverId, from, to);
  }

  public async Task AddPosition(DriverPosition driverPosition)
  {
    await using var transaction = await _transactions.BeginTransaction();
    await _next.AddPosition(driverPosition);
    await transaction.Commit();
  }
}