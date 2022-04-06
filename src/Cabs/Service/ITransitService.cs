using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.Cabs.Service;

public interface ITransitService
{
  Task<Transit> CreateTransit(TransitDto transitDto);
  Task<Transit> CreateTransit(long? clientId, Address from, Address to, CarClasses? carClass);
  Task ChangeTransitAddressFrom(long? transitId, Address newAddress);
  Task ChangeTransitAddressFrom(long? transitId, AddressDto newAddress);
  Task ChangeTransitAddressTo(long? transitId, AddressDto newAddress);
  Task ChangeTransitAddressTo(long? transitId, Address newAddress);
  Task CancelTransit(long? transitId);
  Task<Transit> PublishTransit(long? transitId);
  Task<Transit> FindDriversForTransit(long? transitId);
  Task AcceptTransit(long? driverId, long? transitId);
  Task StartTransit(long? driverId, long? transitId);
  Task RejectTransit(long? driverId, long? transitId);
  Task CompleteTransit(long? driverId, long? transitId, AddressDto destinationAddress);
  Task CompleteTransit(long? driverId, long? transitId, Address destinationAddress);
  Task<TransitDto> LoadTransit(long? id);
}