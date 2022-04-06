using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Pricing;
using NodaTime;

namespace LegacyFighter.CabsTests.Common;

public class StubbedTransitPrice
{
  private readonly Tariffs _tariffs;

  public StubbedTransitPrice(Tariffs tariffs)
  {
    _tariffs = tariffs;
  }

  public void Stub(Money faked)
  {
    var fakeTariff = new Tariff(0, "fake", faked);
    _tariffs.Choose(Arg.Any<Instant>()).Returns(fakeTariff);
  }
}