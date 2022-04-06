using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.Cabs.Ride;

public class TransactionalChangePickupService : IChangePickupService
{
  private readonly IChangePickupService _inner;
  private readonly ITransactions _transactions;

  public TransactionalChangePickupService(
    IChangePickupService inner,
    ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<Distance> ChangeTransitAddressFrom(Guid requestGuid, Address newAddress, Address oldAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    var distance = await _inner.ChangeTransitAddressFrom(requestGuid, newAddress, oldAddress);
    await tx.Commit();
    return distance;
  }

  public async Task ChangeTransitAddressFrom(Guid requestGuid, AddressDto newAddress, AddressDto oldAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.ChangeTransitAddressFrom(requestGuid, newAddress, oldAddress);
    await tx.Commit();
  }
}