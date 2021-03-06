using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.Ride.Details;

namespace LegacyFighter.Cabs.Ride;

public class TransactionalRideService : IRideService
{
  private readonly IRideService _inner;
  private readonly ITransactions _transactions;

  public TransactionalRideService(IRideService inner, ITransactions transactions)
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

  public async Task<TransitDto> CreateTransit(long? clientId, AddressDto fromDto, AddressDto toDto,
    CarClasses? carClass)
  {
    await using var tx = await _transactions.BeginTransaction();
    var transit = await _inner.CreateTransit(clientId, fromDto, toDto, carClass);
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

  public async Task PublishTransit(Guid requestGuid)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.PublishTransit(requestGuid);
    await tx.Commit();
  }

  public async Task<TransitDetailsDto> FindDriversForTransit(Guid requestGuid)
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