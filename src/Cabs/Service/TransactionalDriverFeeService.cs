using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.MoneyValue;

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

  public async Task<Money> CalculateDriverFee(Money transitPrice, long? driverId)
  {
    await using var tx = await _transactions.BeginTransaction();
    var driverFee = await _inner.CalculateDriverFee(transitPrice, driverId);
    await tx.Commit();
    return driverFee;
  }
}