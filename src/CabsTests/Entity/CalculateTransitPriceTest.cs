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

  [Test]
  public void CalculatePriceOnSunday()
  {
    //given
    var transit = ATransit(Transit.Statuses.Completed, 20);
    //and
    TransitWasDoneOnSunday(transit);

    //when
    var price = transit.CalculateFinalCosts();

    //then
    Assert.AreEqual(new Money(3800), price); //38.00
  }

  [Test]
  public void CalculatePriceOnNewYearsEve()
  {
    //given
    var transit = ATransit(Transit.Statuses.Completed, 20);
    //and
    TransitWasDoneOnNewYearsEve(transit);

    //when
    var price = transit.CalculateFinalCosts();

    //then
    Assert.AreEqual(new Money(8100), price); //81.00
  }

  [Test]
  public void CalculatePriceOnSaturday()
  {
    //given
    var transit = ATransit(Transit.Statuses.Completed, 20);
    //and
    TransitWasDoneOnSaturday(transit);

    //when
    var price = transit.CalculateFinalCosts();

    //then
    Assert.AreEqual(new Money(3800), price); //38.00
  }

  [Test]
  public void CalculatePriceOnSaturdayNight()
  {
    //given
    var transit = ATransit(Transit.Statuses.Completed, 20);
    //and
    TransitWasDoneOnSaturdayNight(transit);

    //when
    var price = transit.CalculateFinalCosts();

    //then
    Assert.AreEqual(new Money(6000), price); //60.00
  }

  private static Transit ATransit(Transit.Statuses status, int km)
  {
    return new Transit(status, null, null, null, null, SystemClock.Instance.GetCurrentInstant(), Distance.OfKm(km));
  }

  private static void TransitWasDoneOnNewYearsEve(Transit transit)
  {
    transit.DateTime = new LocalDateTime(2021, 12, 31, 8, 30).InUtc().ToInstant();
  }

  private static void TransitWasDoneOnSaturday(Transit transit)
  {
    transit.DateTime = new LocalDateTime(2021, 4, 17, 8, 30).InUtc().ToInstant();
  }

  private static void TransitWasOnDoneOnFriday(Transit transit)
  {
    transit.DateTime = new LocalDateTime(2021, 4, 16, 8, 30).InUtc().ToInstant();
  }

  private static void TransitWasDoneOnSunday(Transit transit)
  {
    transit.DateTime = new LocalDateTime(2021, 4, 18, 8, 30).InUtc().ToInstant();
  }

  private static void TransitWasDoneOnSaturdayNight(Transit transit)
  {
    transit.DateTime = new LocalDateTime(2021, 4, 17, 19, 30).InUtc().ToInstant();
  }

  private static void TransitWasDoneIn2018(Transit transit)
  {
    transit.DateTime = new LocalDateTime(2018, 1, 1, 8, 30).InUtc().ToInstant();
  }
}