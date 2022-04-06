namespace LegacyFighter.Cabs.Notification;

public interface IDriverNotificationService
{
  void NotifyAboutPossibleTransit(long? driverId, long? transitId);
  void NotifyAboutChangedTransitAddress(long? driverId, long? transitId);
  void NotifyAboutCancelledTransit(long? driverId, long? transitId);
  void AskDriverForDetailsAboutClaim(string claimNo, long? driverId);
}

public class DriverNotificationService : IDriverNotificationService
{
  public void NotifyAboutPossibleTransit(long? driverId, long? transitId)
  {
    // ...
  }

  public void NotifyAboutChangedTransitAddress(long? driverId, long? transitId)
  {
    // ...
  }

  public void NotifyAboutCancelledTransit(long? driverId, long? transitId)
  {
    // ...
  }

  public void AskDriverForDetailsAboutClaim(string claimNo, long? driverId)
  {
    // ...
  }
}