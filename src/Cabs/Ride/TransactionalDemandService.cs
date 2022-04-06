using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Ride;

public class TransactionalDemandService : IDemandService
{
  private readonly IDemandService _inner;
  private readonly ITransactions _transactions;

  public TransactionalDemandService(IDemandService inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task PublishDemand(Guid requestGuid)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.PublishDemand(requestGuid);
    await tx.Commit();

  }

  public async Task CancelDemand(Guid requestGuid)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.CancelDemand(requestGuid);
    await tx.Commit();
  }

  public async Task AcceptDemand(Guid requestGuid)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.AcceptDemand(requestGuid);
    await tx.Commit();
  }

  public async Task<bool> ExistsFor(Guid requestGuid)
  {
    return await _inner.ExistsFor(requestGuid);
  }
}