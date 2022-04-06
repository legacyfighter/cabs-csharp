using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.CabsTests.Common;

public class StubbedTransitPrice 
{
  private readonly ITransitRepository _transitRepository;
  private readonly ITransactions _transactions;

  public StubbedTransitPrice(ITransitRepository transitRepository, ITransactions transactions)
  {
    _transitRepository = transitRepository;
    _transactions = transactions;
  }

  public async Task<Transit> Stub(long? transitId, Money faked)
  {
    await using var tx = await _transactions.BeginTransaction();
    var transit = await _transitRepository.Find(transitId);
    transit.Price = faked;
    await tx.Commit();
    return transit;
  }
}