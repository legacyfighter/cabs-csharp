using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Repair.Legacy.Job;

namespace LegacyFighter.Cabs.Repair.Legacy.Service;

public class TransactionalJobDoer : IJobDoer
{
  private readonly IJobDoer _inner;
  private readonly ITransactions _transactions;

  public TransactionalJobDoer(IJobDoer inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<JobResult> Repair(long? userId, CommonBaseAbstractJob job)
  {
    await using var transaction = await _transactions.BeginTransaction();
    var result = await _inner.Repair(userId, job);
    await transaction.Commit();
    return result;
  }
}