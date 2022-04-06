namespace LegacyFighter.Cabs.Notification;

public interface IDriverNotificationService
{
  void NotifyAboutPossibleTransit(long? driverId, long? transitId);
  void NotifyAboutChangedTransitAddress(long? driverId, long? transitId);
  void NotifyAboutCancelledTransit(long? driverId, long? transitId);
  void NotifyAboutPossibleTransit(long? driverId, Guid requestId);
  void NotifyAboutChangedTransitAddress(long? driverId, Guid requestId);
  void NotifyAboutCancelledTransit(long? driverId, Guid requestId);
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

  public void NotifyAboutPossibleTransit(long? driverId, Guid requestId)
  {
    // find transit and delegate to NotifyAboutPossibleTransit(long?, long?)
  }

  public void NotifyAboutChangedTransitAddress(long? driverId, Guid requestId)
  {
    // find transit and delegate to NotifyAboutChangedTransitAddress(long?, long?)
  }

  public void NotifyAboutCancelledTransit(long? driverId, Guid requestId)
  {
    // find transit and delegate to NotifyAboutCancelledTransit(long?, long?)
  }

  public void AskDriverForDetailsAboutClaim(string claimNo, long? driverId)
  {
    // ...
  }
}