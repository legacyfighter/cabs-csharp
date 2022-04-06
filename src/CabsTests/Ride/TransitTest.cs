using System;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Pricing;
using LegacyFighter.Cabs.Ride;
using NodaTime;

namespace LegacyFighter.CabsTests.Ride;

internal class TransitTest
{
  [Test]
  public void CanChangeTransitDestination()
  {
    //given
    var transit = NewTransit();

    //expect
    transit.ChangeDestination(Distance.OfKm(20));

    //then
    Assert.AreEqual(Distance.OfKm(20), transit.Distance);
  }

  [Test]
  public void CannotChangeDestinationWhenTransitIsCompleted()
  {
    //given
    var transit = NewTransit();
    //and
    transit.CompleteTransitAt(Distance.OfKm(20));

    //expect
    transit.Invoking(t => t.ChangeDestination(Distance.OfKm(20)))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void CanCompleteTransit()
  {
    var transit = NewTransit();
    //and
    transit.CompleteTransitAt(Distance.OfKm(20));

    //then
    Assert.AreEqual(Transit.Statuses.Completed, transit.Status);
  }

  Transit NewTransit()
  {
    return new Transit(
      Tariff.OfTime(SystemClock.Instance.GetCurrentInstant().InUtc().LocalDateTime),
      Guid.NewGuid());
  }
}