using NodaTime;

namespace LegacyFighter.Cabs.Pricing;

public class Tariffs
{
  public virtual Tariff Choose(Instant? when)
  {
    if (when == null)
    {
      when = SystemClock.Instance.GetCurrentInstant();
    }

    return Tariff.OfTime(when.Value.InZone(DateTimeZoneProviders.Bcl.GetSystemDefault()).LocalDateTime);
  }
}