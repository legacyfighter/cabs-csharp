using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Geolocation.Address;
using NodaTime;

namespace LegacyFighter.Cabs.Assignment;

public interface IDriverAssignmentFacade
{
  Task<InvolvedDriversSummary> StartAssigningDrivers(Guid transitRequestGuid,
    AddressDto from,
    CarClasses? carClass,
    Instant when);

  Task<InvolvedDriversSummary> SearchForPossibleDrivers(Guid transitRequestGuid,
    AddressDto from,
    CarClasses? carClass);

  Task<InvolvedDriversSummary> AcceptTransit(Guid transitRequestGuid, long? driverId);
  Task<InvolvedDriversSummary> RejectTransit(Guid transitRequestGuid, long? driverId);
  Task<bool> IsDriverAssigned(Guid transitRequestGuid);
  Task<InvolvedDriversSummary> LoadInvolvedDrivers(Guid transitRequestGuid);
  Task Cancel(Guid transitRequestGuid);
  Task NotifyAssignedDriverAboutChangedDestination(Guid transitRequestGuid);
  Task NotifyProposedDriversAboutChangedDestination(Guid transitRequestGuid);
}