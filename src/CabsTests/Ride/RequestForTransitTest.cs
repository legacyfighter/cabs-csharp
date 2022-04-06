using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Pricing;
using LegacyFighter.Cabs.Ride;
using NodaTime;

namespace LegacyFighter.CabsTests.Ride;

public class RequestForTransitTest
{
  [Test]
  public void CanCreateRequestForTransit()
  {
    //when
    var requestForTransit = RequestTransit();

    //expect
    Assert.NotNull(requestForTransit.Tariff);
    Assert.AreNotEqual(0, requestForTransit.Tariff.KmRate);
  }

  private RequestForTransit RequestTransit()
  {
    var tariff = Tariff.OfTime(SystemClock.Instance.GetCurrentInstant().InUtc().LocalDateTime);
    return new RequestForTransit(tariff, Distance.Zero);
  }
}