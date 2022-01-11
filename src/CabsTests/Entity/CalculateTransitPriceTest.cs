using System;
using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;
using NodaTime;

namespace LegacyFighter.CabsTests.Entity;

public class CalculateTransitPriceTest
{
  [Test]
  public void CannotCalculatePriceWhenTransitIsCancelled()
  {
    //given
    var transit = ATransit(Transit.Statuses.Cancelled, 20);

    //expect
    transit.Invoking(t => t.CalculateFinalCosts())
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void CannotEstimatePriceWhenTransitIsCompleted()
  {
    //given
    var transit = ATransit(Transit.Statuses.Completed, 20);

    //expect
    transit.Invoking(t => t.EstimateCost())
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void CalculatePriceOnRegularDay()
  {
    //given
    var transit = ATransit(Transit.Statuses.Completed, 20);

    //friday
    TransitWasOnDoneOnFriday(transit);
    //when
    var price = transit.CalculateFinalCosts();

    //then
    Assert.AreEqual(new Money(2900), price); //29.00
  }

  [Test]
  public void EstimatePriceOnRegularDay()
  {
    //given
    var transit = ATransit(Transit.Statuses.Draft, 20);

    //friday
    TransitWasOnDoneOnFriday(transit);
    //when
    var price = transit.EstimateCost();

    //then
    Assert.AreEqual(new Money(2900), price); //29.00
  }

  private static Transit ATransit(Transit.Statuses status, int km)
  {
    var transit = new Transit();
    transit.DateTime = SystemClock.Instance.GetCurrentInstant();
    transit.Status = Transit.Statuses.Draft;
    transit.KmDistance = Distance.OfKm(km);
    transit.Status = status;
    return transit;
  }

  private static void TransitWasOnDoneOnFriday(Transit transit)
  {
    transit.DateTime = new LocalDateTime(2021, 4, 16, 8, 30).InUtc().ToInstant();
  }
}