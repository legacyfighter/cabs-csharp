using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;
using NodaTime;

namespace LegacyFighter.Cabs.TransitDetail;

public class TransactionalTransitDetailsFacade : ITransitDetailsFacade
{
  private readonly ITransitDetailsFacade _inner;
  private readonly ITransactions _transactions;

  public TransactionalTransitDetailsFacade(
    ITransitDetailsFacade inner, 
    ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<TransitDetailsDto> Find(long? transitId)
  {
    return await _inner.Find(transitId);
  }

  public async Task TransitRequested(Instant when, long? transitId, Address from, Address to, Distance distance, Client client,
    CarClasses? carClass, Money estimatedPrice, Tariff tariff)
  {
    await _inner.TransitRequested(when, transitId, from, to, distance, client, carClass, estimatedPrice, tariff);
  }

  public async Task PickupChangedTo(long? transitId, Address newAddress, Distance newDistance)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.PickupChangedTo(transitId, newAddress, newDistance);
    await tx.Commit();
  }

  public async Task DestinationChanged(long? transitId, Address newAddress, Distance newDistance)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.DestinationChanged(transitId, newAddress, newDistance);
    await tx.Commit();
  }

  public async Task TransitPublished(long? transitId, Instant when)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.TransitPublished(transitId, when);
    await tx.Commit();
  }

  public async Task TransitStarted(long? transitId, Instant when)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.TransitStarted(transitId, when);
    await tx.Commit();
  }

  public async Task TransitAccepted(long? transitId, Instant when, long? driverId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.TransitAccepted(transitId, when, driverId);
    await tx.Commit();
  }

  public async Task TransitCancelled(long? transitId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.TransitCancelled(transitId);
    await tx.Commit();
  }

  public async Task TransitCompleted(long? transitId, Instant when, Money price, Money driverFee)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.TransitCompleted(transitId, when, price, driverFee);
    await tx.Commit();
  }

  public async Task<List<TransitDetailsDto>> FindByClient(long? clientId)
  {
    return await _inner.FindByClient(clientId);
  }

  public async Task<List<TransitDetailsDto>> FindByDriver(long? driverId, Instant from, Instant to)
  {
    return await _inner.FindByDriver(driverId, from, to);
  }

  public async Task<List<TransitDetailsDto>> FindCompleted()
  {
    return await _inner.FindCompleted();
  }
}