namespace LegacyFighter.Cabs.Notification;

public static class NotificationDependencies
{
  public static void AddTo(WebApplicationBuilder builder)
  {
    builder.Services.AddTransient<IClientNotificationService, ClientNotificationService>();
    builder.Services.AddTransient<IDriverNotificationService, DriverNotificationService>();
  }
}