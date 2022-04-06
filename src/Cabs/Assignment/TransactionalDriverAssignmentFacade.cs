using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Geolocation.Address;
using NodaTime;

namespace LegacyFighter.Cabs.Assignment;

public class TransactionalDriverAssignmentFacade : IDriverAssignmentFacade
{
  private readonly IDriverAssignmentFacade _inner;
  private readonly ITransactions _transactions;

  public TransactionalDriverAssignmentFacade(
    IDriverAssignmentFacade inner,
    ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<InvolvedDriversSummary> StartAssigningDrivers(Guid transitRequestGuid, AddressDto from,
    CarClasses? carClass, Instant when)
  {    
    await using var tx = await _transactions.BeginTransaction();
    var involvedDriversSummary = await _inner.StartAssigningDrivers(transitRequestGuid, from, carClass, when);
    await tx.Commit();
    return involvedDriversSummary;
  }

  public async Task<InvolvedDriversSummary> SearchForPossibleDrivers(Guid transitRequestGuid, AddressDto from,
    CarClasses? carClass)
  {
    await using var tx = await _transactions.BeginTransaction();
    var drivers = await _inner.SearchForPossibleDrivers(transitRequestGuid, from, carClass);
    await tx.Commit();
    return drivers;
  }

  public async Task<InvolvedDriversSummary> AcceptTransit(Guid transitRequestGuid, long? driverId)
  {    
    await using var tx = await _transactions.BeginTransaction();
    var involvedDriversSummary = await _inner.AcceptTransit(transitRequestGuid, driverId);
    await tx.Commit();
    return involvedDriversSummary;
  }

  public async Task<InvolvedDriversSummary> RejectTransit(Guid transitRequestGuid, long? driverId)
  {
    await using var tx = await _transactions.BeginTransaction();
    var involvedDriversSummary = await _inner.RejectTransit(transitRequestGuid, driverId);
    await tx.Commit();
    return involvedDriversSummary;
  }

  public async Task<bool> IsDriverAssigned(Guid transitRequestGuid)
  {
    return await _inner.IsDriverAssigned(transitRequestGuid);
  }

  public async Task<InvolvedDriversSummary> LoadInvolvedDrivers(Guid transitRequestGuid)
  {
    return await _inner.LoadInvolvedDrivers(transitRequestGuid);
  }

  public async Task Cancel(Guid transitRequestGuid)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.Cancel(transitRequestGuid);
    await tx.Commit();
  }

  public async Task NotifyAssignedDriverAboutChangedDestination(Guid transitRequestGuid)
  {
    await _inner.NotifyAssignedDriverAboutChangedDestination(transitRequestGuid);
  }

  public async Task NotifyProposedDriversAboutChangedDestination(Guid transitRequestGuid)
  {
    await _inner.NotifyProposedDriversAboutChangedDestination(transitRequestGuid);
  }
}