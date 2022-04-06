using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Crm.TransitAnalyzer;

public class TransactionalPopulateGraphService : IPopulateGraphService
{
  private readonly IPopulateGraphService _inner;
  private readonly ITransactions _transactions;

  public TransactionalPopulateGraphService(IPopulateGraphService inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task Populate()
  {
    await using var transaction = await _transactions.BeginTransaction();
    await _inner.Populate();
    await transaction.Commit();
  }
}