using NodaTime;

namespace LegacyFighter.Cabs.Entity.Miles;

public interface IMiles 
{
  int? GetAmountFor(Instant moment);
  IMiles Subtract(int? amount, Instant moment);
  Instant ExpiresAt();
}