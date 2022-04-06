using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public class TransactionalTransitService : ITransitService
{
  private readonly ITransitService _inner;
  private readonly ITransactions _transactions;

  public TransactionalTransitService(ITransitService inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<Transit> CreateTransit(TransitDto transitDto)
  {
    await using var tx = await _transactions.BeginTransaction();
    var transit = await _inner.CreateTransit(transitDto);
    await tx.Commit();
    return transit;
  }

  public async Task<Transit> CreateTransit(long? clientId, Address from, Address to, CarClasses? carClass)
  {
    await using var tx = await _transactions.BeginTransaction();
    var transit = await _inner.CreateTransit(clientId, from, to, carClass);
    await tx.Commit();
    return transit;
  }

  public async Task ChangeTransitAddressFrom(long? transitId, Address newAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.ChangeTransitAddressFrom(transitId, newAddress);
    await tx.Commit();
  }

  public async Task ChangeTransitAddressFrom(long? transitId, AddressDto newAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.ChangeTransitAddressFrom(transitId, newAddress);
    await tx.Commit();
  }

  public async Task ChangeTransitAddressTo(long? transitId, AddressDto newAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.ChangeTransitAddressTo(transitId, newAddress);
    await tx.Commit();
  }

  public async Task ChangeTransitAddressTo(long? transitId, Address newAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.ChangeTransitAddressTo(transitId, newAddress);
    await tx.Commit();
  }

  public async Task CancelTransit(long? transitId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.CancelTransit(transitId);
    await tx.Commit();
  }

  public async Task<Transit> PublishTransit(long? transitId)
  {
    await using var tx = await _transactions.BeginTransaction();
    var transit = await _inner.PublishTransit(transitId);
    await tx.Commit();
    return transit;
  }

  public async Task<Transit> FindDriversForTransit(long? transitId)
  {
    await using var tx = await _transactions.BeginTransaction();
    var transit = await _inner.FindDriversForTransit(transitId);
    await tx.Commit();
    return transit;
  }

  public async Task AcceptTransit(long? driverId, long? transitId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.AcceptTransit(driverId, transitId);
    await tx.Commit();
  }

  public async Task StartTransit(long? driverId, long? transitId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.StartTransit(driverId, transitId);
    await tx.Commit();
  }

  public async Task RejectTransit(long? driverId, long? transitId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.RejectTransit(driverId, transitId);
    await tx.Commit();
  }

  public async Task CompleteTransit(long? driverId, long? transitId, AddressDto destinationAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.CompleteTransit(driverId, transitId, destinationAddress);
    await tx.Commit();
  }

  public async Task CompleteTransit(long? driverId, long? transitId, Address destinationAddress)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.CompleteTransit(driverId, transitId, destinationAddress);
    await tx.Commit();
  }
  
  public async Task<TransitDto> LoadTransit(long? id)
  {
    await using var tx = await _transactions.BeginTransaction();
    var transit = await _inner.LoadTransit(id);
    await tx.Commit();
    return transit;
  }
}