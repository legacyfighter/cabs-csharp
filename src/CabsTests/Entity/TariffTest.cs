using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;
using NodaTime;

namespace LegacyFighter.CabsTests.Entity;

public class TariffTest
{
  [Test]
  public void RegularTariffShouldBeDisplayedAndCalculated()
  {
    //given
    var tariff = Tariff.OfTime(new LocalDateTime(2021, 4, 16, 8, 30));

    //expect
    Assert.AreEqual(new Money(2900), tariff.CalculateCost(Distance.OfKm(20))); //29.00
    Assert.AreEqual("Standard", tariff.Name);
    Assert.AreEqual(1.0f, tariff.KmRate);
  }

  [Test]
  public void SundayTariffShouldBeDisplayedAndCalculated()
  {
    //expect
    var tariff = Tariff.OfTime(new LocalDateTime(2021, 4, 18, 8, 30));

    //expect
    Assert.AreEqual(new Money(3800), tariff.CalculateCost(Distance.OfKm(20))); //38.00
    Assert.AreEqual("Weekend", tariff.Name);
    Assert.AreEqual(1.5f, tariff.KmRate);
  }

  [Test]
  public void NewYearsEveTariffShouldBeDisplayedAndCalculated()
  {
    //given
    var tariff = Tariff.OfTime(new LocalDateTime(2021, 12, 31, 8, 30));

    //expect
    Assert.AreEqual(new Money(8100), tariff.CalculateCost(Distance.OfKm(20))); //81.00
    Assert.AreEqual("Sylwester", tariff.Name);
    Assert.AreEqual(3.5f, tariff.KmRate);
  }

  [Test]
  public void SaturdayTariffShouldBeDisplayedAndCalculated()
  {
    //given
    var tariff = Tariff.OfTime(new LocalDateTime(2021, 4, 17, 8, 30));

    //expect
    Assert.AreEqual(new Money(3800), tariff.CalculateCost(Distance.OfKm(20))); //38.00
    Assert.AreEqual("Weekend", tariff.Name);
    Assert.AreEqual(1.5f, tariff.KmRate);
  }

  [Test]
  public void SaturdayNightTariffShouldBeDisplayedAndCalculated()
  {
    //given
    var tariff = Tariff.OfTime(new LocalDateTime(2021, 4, 17, 19, 30));

    //expect
    Assert.AreEqual(new Money(6000), tariff.CalculateCost(Distance.OfKm(20))); //60.00
    Assert.AreEqual("Weekend+", tariff.Name);
    Assert.AreEqual(2.5f, tariff.KmRate);
  }
}