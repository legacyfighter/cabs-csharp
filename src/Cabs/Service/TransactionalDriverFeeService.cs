using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Service;

public class TransactionalDriverFeeService : IDriverFeeService
{
  private readonly IDriverFeeService _inner;
  private readonly ITransactions _transactions;

  public TransactionalDriverFeeService(IDriverFeeService inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<int> CalculateDriverFee(long? transitId)
  {
    await using var tx = await _transactions.BeginTransaction();
    var driverFee = await _inner.CalculateDriverFee(transitId);
    await tx.Commit();
    return driverFee;
  }
}