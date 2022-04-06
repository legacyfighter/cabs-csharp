using LegacyFighter.Cabs.Assignment;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Pricing;
using NodaTime;

namespace LegacyFighter.Cabs.Ride.Details;

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

  public async Task<TransitDetailsDto> Find(Guid requestId)
  {
    return await _inner.Find(requestId);
  }

  public async Task TransitRequested(Instant when, Guid requestId, Address from, Address to, Distance distance,
    Client client,
    CarClasses? carClass, Money estimatedPrice, Tariff tariff)
  {
    await _inner.TransitRequested(when, requestId, from, to, distance, client, carClass, estimatedPrice, tariff);
  }

  public async Task PickupChangedTo(Guid requestId, Address newAddress, Distance newDistance)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.PickupChangedTo(requestId, newAddress, newDistance);
    await tx.Commit();
  }

  public async Task DestinationChanged(Guid requestId, Address newAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.DestinationChanged(requestId, newAddress);
    await tx.Commit();
  }

  public async Task TransitPublished(Guid requestId, Instant when)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.TransitPublished(requestId, when);
    await tx.Commit();
  }

  public async Task TransitStarted(Guid requestId, long? transitId, Instant when)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.TransitStarted(requestId, transitId, when);
    await tx.Commit();
  }

  public async Task TransitAccepted(Guid requestId, long? driverId, Instant when)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.TransitAccepted(requestId, driverId, when);
    await tx.Commit();
  }

  public async Task TransitCancelled(Guid requestId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.TransitCancelled(requestId);
    await tx.Commit();
  }

  public async Task TransitCompleted(Guid requestId, Instant when, Money price, Money driverFee)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.TransitCompleted(requestId, when, price, driverFee);
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

  public async Task DriversAreInvolved(Guid requestId, InvolvedDriversSummary involvedDriversSummary)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.DriversAreInvolved(requestId, involvedDriversSummary);
    await tx.Commit();
  }
}