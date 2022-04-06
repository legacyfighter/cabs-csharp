using NodaTime;

namespace LegacyFighter.Cabs.Loyalty;

public interface IMiles 
{
  int? GetAmountFor(Instant moment);
  IMiles Subtract(int? amount, Instant moment);
  Instant ExpiresAt();
}