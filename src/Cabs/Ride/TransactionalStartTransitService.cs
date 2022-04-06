using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Ride;

public class TransactionalStartTransitService : IStartTransitService
{
  private readonly IStartTransitService _inner;
  private readonly ITransactions _transactions;

  public TransactionalStartTransitService(
    IStartTransitService inner,
    ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<Transit> Start(Guid requestGuid)
  {    
    await using var tx = await _transactions.BeginTransaction();
    var transit = await _inner.Start(requestGuid);
    await tx.Commit();
    return transit;
  }
}