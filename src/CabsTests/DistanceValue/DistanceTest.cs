using System;
using LegacyFighter.Cabs.DistanceValue;

namespace LegacyFighter.CabsTests.DistanceValue;

public class DistanceTest
{
  [Test]
  public void CannotUnderstandInvalidUnit()
  {
    //expect
    Distance.OfKm(2000).Invoking(d => d.PrintIn("invalid"))
      .Should().ThrowExactly<ArgumentException>();
  }

  [Test]
  public void CanConvertToFloat()
  {
    //expect
    Assert.AreEqual(2000f, Distance.OfKm(2000).ToKmInFloat());
    Assert.AreEqual(0f, Distance.OfKm(0).ToKmInFloat());
    Assert.AreEqual(312.22f, Distance.OfKm(312.22f).ToKmInFloat());
    Assert.AreEqual(2f, Distance.OfKm(2).ToKmInFloat());
  }

  [Test]
  public void CanConvertToDouble()
  {
    //expect
    Assert.AreEqual(2000d, Distance.OfKm(2000).ToKmInDouble());
    Assert.AreEqual(0d, Distance.OfKm(0).ToKmInDouble());
    Assert.AreEqual(312.22d, Distance.OfKm(312.22d).ToKmInDouble());
    Assert.AreEqual(2d, Distance.OfKm(2).ToKmInDouble());
  }

  [Test]
  public void CanRepresentDistanceAsMeters()
  {
    //expect
    Assert.AreEqual("2000000m", Distance.OfKm(2000).PrintIn("m"));
    Assert.AreEqual("0m", Distance.OfKm(0).PrintIn("m"));
    Assert.AreEqual("312220m", Distance.OfKm(312.22f).PrintIn("m"));
    Assert.AreEqual("2000m", Distance.OfKm(2).PrintIn("m"));
  }

  [Test]
  public void CanRepresentDistanceAsKm()
  {
    //expect
    Assert.AreEqual("2000km", Distance.OfKm(2000).PrintIn("km"));
    Assert.AreEqual("0km", Distance.OfKm(0).PrintIn("km"));
    Assert.AreEqual("312.220km", Distance.OfKm(312.22f).PrintIn("km"));
    Assert.AreEqual("312.221km", Distance.OfKm(312.221111232313f).PrintIn("km"));
    Assert.AreEqual("2km", Distance.OfKm(2).PrintIn("km"));
  }

  [Test]
  public void CanRepresentDistanceAsMiles()
  {
    //expect
    Assert.AreEqual("1242.742miles", Distance.OfKm(2000).PrintIn("miles"));
    Assert.AreEqual("0miles", Distance.OfKm(0).PrintIn("miles"));
    Assert.AreEqual("194.005miles", Distance.OfKm(312.22f).PrintIn("miles"));
    Assert.AreEqual("194.005miles", Distance.OfKm(312.221111232313f).PrintIn("miles"));
    Assert.AreEqual("1.243miles", Distance.OfKm(2).PrintIn("miles"));
  }

  [Test]
  public void CanAddDistances()
  {
    //expect
    Assert.AreEqual(Distance.OfKm(1000f), Distance.OfKm(500f).Add(Distance.OfKm(500f)));
    Assert.AreEqual(Distance.OfKm(1042f), Distance.OfKm(1020f).Add(Distance.OfKm(22f)));
    Assert.AreEqual(Distance.OfKm(0f), Distance.OfKm(0f).Add(Distance.OfKm(0f)));
    Assert.AreEqual(Distance.OfKm(3.7f), Distance.OfKm(1.5f).Add(Distance.OfKm(2.2f)));
  }
}