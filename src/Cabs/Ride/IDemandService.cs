namespace LegacyFighter.Cabs.Ride;

public interface IDemandService
{
  Task PublishDemand(Guid requestGuid);
  Task CancelDemand(Guid requestGuid);
  Task AcceptDemand(Guid requestGuid);
  Task<bool> ExistsFor(Guid requestGuid);
}