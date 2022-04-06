using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.Cabs.Ride;

public interface ITransitService
{
  Task<TransitDto> CreateTransit(TransitDto transitDto);
  Task<TransitDto> CreateTransit(long? clientId, Address from, Address to, CarClasses? carClass);
  Task ChangeTransitAddressFrom(Guid requestGuid, Address newAddress);
  Task ChangeTransitAddressFrom(Guid requestGuid, AddressDto newAddress);
  Task ChangeTransitAddressTo(Guid requestGuid, AddressDto newAddress);
  Task ChangeTransitAddressTo(Guid requestGuid, Address newAddress);
  Task CancelTransit(Guid requestGuid);
  Task<Transit> PublishTransit(Guid requestGuid);
  Task<Transit> FindDriversForTransit(Guid requestGuid);
  Task AcceptTransit(long? driverId, Guid requestGuid);
  Task StartTransit(long? driverId, Guid requestGuid);
  Task RejectTransit(long? driverId, Guid requestGuid);
  Task CompleteTransit(long? driverId, Guid requestGuid, AddressDto destinationAddress);
  Task CompleteTransit(long? driverId, Guid requestGuid, Address destinationAddress);
  Task<TransitDto> LoadTransit(Guid requestGuid);
  Task<TransitDto> LoadTransit(long? requestId);
  Task<Guid> GetRequestGuid(long? requestId);
}