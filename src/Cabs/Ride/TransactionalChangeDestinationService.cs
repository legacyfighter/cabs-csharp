using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.Cabs.Ride;

public class TransactionalChangeDestinationService : IChangeDestinationService
{
  private readonly IChangeDestinationService _inner;
  private readonly ITransactions _transactions;

  public TransactionalChangeDestinationService(
    IChangeDestinationService inner,
    ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<Distance> ChangeTransitAddressTo(Guid requestGuid, Address newAddress, Address from)
  {
    await using var tx = await _transactions.BeginTransaction();
    var distance = await _inner.ChangeTransitAddressTo(requestGuid, newAddress, from);
    await tx.Commit();
    return distance;
  }
}