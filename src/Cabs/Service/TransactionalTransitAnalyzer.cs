using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public class TransactionalTransitAnalyzer : ITransitAnalyzer
{
  private readonly ITransitAnalyzer _inner;
  private readonly ITransactions _transactions;

  public TransactionalTransitAnalyzer(ITransitAnalyzer inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<List<Address>> Analyze(long? clientId, long? addressId)
  {
    await using var tx = await _transactions.BeginTransaction();
    var addresses = await _inner.Analyze(clientId, addressId);
    await tx.Commit();
    return addresses;
  }
}