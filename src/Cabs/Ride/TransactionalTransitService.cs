using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.Cabs.Ride;

public class TransactionalTransitService : ITransitService
{
  private readonly ITransitService _inner;
  private readonly ITransactions _transactions;

  public TransactionalTransitService(ITransitService inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<TransitDto> CreateTransit(TransitDto transitDto)
  {
    await using var tx = await _transactions.BeginTransaction();
    var transit = await _inner.CreateTransit(transitDto);
    await tx.Commit();
    return transit;
  }

  public async Task<TransitDto> CreateTransit(long? clientId, Address from, Address to, CarClasses? carClass)
  {
    await using var tx = await _transactions.BeginTransaction();
    var transit = await _inner.CreateTransit(clientId, from, to, carClass);
    await tx.Commit();
    return transit;
  }

  public async Task ChangeTransitAddressFrom(Guid requestGuid, Address newAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.ChangeTransitAddressFrom(requestGuid, newAddress);
    await tx.Commit();
  }

  public async Task ChangeTransitAddressFrom(Guid requestGuid, AddressDto newAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.ChangeTransitAddressFrom(requestGuid, newAddress);
    await tx.Commit();
  }

  public async Task ChangeTransitAddressTo(Guid requestGuid, AddressDto newAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.ChangeTransitAddressTo(requestGuid, newAddress);
    await tx.Commit();
  }

  public async Task ChangeTransitAddressTo(Guid requestGuid, Address newAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.ChangeTransitAddressTo(requestGuid, newAddress);
    await tx.Commit();
  }

  public async Task CancelTransit(Guid requestGuid)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.CancelTransit(requestGuid);
    await tx.Commit();
  }

  public async Task<Transit> PublishTransit(Guid requestGuid)
  {
    await using var tx = await _transactions.BeginTransaction();
    var transit = await _inner.PublishTransit(requestGuid);
    await tx.Commit();
    return transit;
  }

  public async Task<Transit> FindDriversForTransit(Guid requestGuid)
  {
    await using var tx = await _transactions.BeginTransaction();
    var transit = await _inner.FindDriversForTransit(requestGuid);
    await tx.Commit();
    return transit;
  }

  public async Task AcceptTransit(long? driverId, Guid requestGuid)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.AcceptTransit(driverId, requestGuid);
    await tx.Commit();
  }

  public async Task StartTransit(long? driverId, Guid requestGuid)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.StartTransit(driverId, requestGuid);
    await tx.Commit();
  }

  public async Task RejectTransit(long? driverId, Guid requestGuid)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.RejectTransit(driverId, requestGuid);
    await tx.Commit();
  }

  public async Task CompleteTransit(long? driverId, Guid requestGuid, AddressDto destinationAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.CompleteTransit(driverId, requestGuid, destinationAddress);
    await tx.Commit();
  }

  public async Task CompleteTransit(long? driverId, Guid requestGuid, Address destinationAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.CompleteTransit(driverId, requestGuid, destinationAddress);
    await tx.Commit();
  }
  
  public async Task<TransitDto> LoadTransit(Guid requestGuid)
  {
    await using var tx = await _transactions.BeginTransaction();
    var transit = await _inner.LoadTransit(requestGuid);
    await tx.Commit();
    return transit;
  }

  public async Task<TransitDto> LoadTransit(long? requestId)
  {
    return await _inner.LoadTransit(requestId);
  }

  public async Task<Guid> GetRequestGuid(long? requestId)
  {
    return await _inner.GetRequestGuid(requestId);
  }
}