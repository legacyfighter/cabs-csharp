namespace LegacyFighter.Cabs.Ride;

public interface IStartTransitService
{
  Task<Transit> Start(Guid requestGuid);
}