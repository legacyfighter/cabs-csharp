namespace LegacyFighter.Cabs.Service;

public interface IClientNotificationService
{
  void NotifyClientAboutRefund(string claimNo, long? clientId);
  void AskForMoreInformation(string claimNo, long? clientId);
}

public class ClientNotificationService : IClientNotificationService
{

  public void NotifyClientAboutRefund(string claimNo, long? clientId)
  {

  }

  public void AskForMoreInformation(string claimNo, long? clientId)
  {

  }

}