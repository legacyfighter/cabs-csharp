using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;

namespace LegacyFighter.Cabs.Ride;

public class TransactionalCompleteTransitService : ICompleteTransitService
{
  private readonly ICompleteTransitService _inner;
  private readonly ITransactions _transactions;

  public TransactionalCompleteTransitService(
    ICompleteTransitService inner,
    ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<Money> CompleteTransit(long? driverId, Guid requestGuid, Address from, Address to)
  {    
    await using var tx = await _transactions.BeginTransaction();
    var money = await _inner.CompleteTransit(driverId, requestGuid, from, to);
    await tx.Commit();
    return money;
  }
}