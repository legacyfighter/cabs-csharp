using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Pricing;
using LegacyFighter.Cabs.Ride;
using NodaTime;

namespace LegacyFighter.CabsTests.Pricing;

internal class CalculateTransitPriceTest
{
  [Test]
  public void CalculatePriceOnRegularDay()
  {
    //given
    //friday
    var requestForTransit = TransitWasOnDoneOnFriday(Distance.OfKm(20));
    //when
    var price = requestForTransit.EstimatedPrice;

    //then
    Assert.AreEqual(new Money(2900), price); //29.00
  }

  [Test]
  public void CalculatePriceOnSunday()
  {
    //given
    var requestForTransit = TransitWasDoneOnSunday(Distance.OfKm(20));

    //when
    var price = requestForTransit.EstimatedPrice;

    //then
    Assert.AreEqual(new Money(3800), price); //38.00
  }

  [Test]
  public void CalculatePriceOnNewYearsEve()
  {
    //given
    var requestForTransit = TransitWasDoneOnNewYearsEve(Distance.OfKm(20));

    //when
    var price = requestForTransit.EstimatedPrice;

    //then
    Assert.AreEqual(new Money(8100), price); //81.00
  }

  [Test]
  public void CalculatePriceOnSaturday()
  {
    //given
    var requestForTransit = TransitWasDoneOnSaturday(Distance.OfKm(20));

    //when
    var price = requestForTransit.EstimatedPrice;

    //then
    Assert.AreEqual(new Money(3800), price); //38.00
  }

  [Test]
  public void CalculatePriceOnSaturdayNight()
  {
    //given
    var requestForTransit = TransitWasDoneOnSaturdayNight(Distance.OfKm(20));

    //when
    var price = requestForTransit.EstimatedPrice;

    //then
    Assert.AreEqual(new Money(6000), price); //60.00
  }

  private RequestForTransit TransitWasOnDoneOnFriday(Distance distance)
  {
    var tariff = Tariff.OfTime(new LocalDateTime(2021, 4, 16, 8, 30));
    var requestForTransit = new RequestForTransit(tariff, distance);
    return requestForTransit;
  }

  private RequestForTransit TransitWasDoneOnNewYearsEve(Distance distance)
  {
    var tariff = Tariff.OfTime(new LocalDateTime(2021, 12, 31, 8, 30));
    var requestForTransit = new RequestForTransit(tariff, distance);
    return requestForTransit;
  }

  private RequestForTransit TransitWasDoneOnSaturday(Distance distance)
  {
    var tariff = Tariff.OfTime(new LocalDateTime(2021, 4, 17, 8, 30));
    var requestForTransit = new RequestForTransit(tariff, distance);
    return requestForTransit;
  }

  private RequestForTransit TransitWasDoneOnSunday(Distance distance)
  {
    var tariff = Tariff.OfTime(new LocalDateTime(2021, 4, 18, 8, 30));
    var requestForTransit = new RequestForTransit(tariff, distance);
    return requestForTransit;
  }

  private RequestForTransit TransitWasDoneOnSaturdayNight(Distance distance)
  {
    var tariff = Tariff.OfTime(new LocalDateTime(2021, 4, 17, 19, 30));
    var requestForTransit = new RequestForTransit(tariff, distance);
    return requestForTransit;
  }
}