using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.Cabs.Ride;

public class TransactionalRequestTransitService : IRequestTransitService
{
  private readonly IRequestTransitService _inner;
  private readonly ITransactions _transactions;

  public TransactionalRequestTransitService(
    IRequestTransitService inner,
    ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<RequestForTransit> CreateRequestForTransit(Address from, Address to)
  {
    await using var tx = await _transactions.BeginTransaction();
    var requestForTransit = await _inner.CreateRequestForTransit(from, to);
    await tx.Commit();
    return requestForTransit;
  }
}