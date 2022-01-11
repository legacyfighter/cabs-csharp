using System;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;
using NodaTime;
using NodaTime.Extensions;

namespace LegacyFighter.CabsTests.Ui;

public class CalculateTransitDistanceTest
{
  [Test]
  public void ShouldNotWorkWithInvalidUnit()
  {
    TransitForDistance(2f).Invoking(t => t.GetDistance("invalid"))
      .Should().ThrowExactly<ArgumentException>();
  }

  [Test]
  public void ShouldRepresentAsKm()
  {
    Assert.AreEqual("10km", TransitForDistance(10).GetDistance("km"));
    Assert.AreEqual("10.123km", TransitForDistance(10.123f).GetDistance("km"));
    Assert.AreEqual("10.123km", TransitForDistance(10.12345f).GetDistance("km"));
    Assert.AreEqual("0km", TransitForDistance(0).GetDistance("km"));
  }

  [Test]
  public void ShouldRepresentAsMeters()
  {
    Assert.AreEqual("10000m", TransitForDistance(10).GetDistance("m"));
    Assert.AreEqual("10123m", TransitForDistance(10.123f).GetDistance("m"));
    Assert.AreEqual("10123m", TransitForDistance(10.12345f).GetDistance("m"));
    Assert.AreEqual("0m", TransitForDistance(0).GetDistance("m"));
  }

  [Test]
  public void ShouldRepresentAsMiles()
  {
    Assert.AreEqual("6.214miles", TransitForDistance(10).GetDistance("miles"));
    Assert.AreEqual("6.290miles", TransitForDistance(10.123f).GetDistance("miles"));
    Assert.AreEqual("6.290miles", TransitForDistance(10.12345f).GetDistance("miles"));
    Assert.AreEqual("0miles", TransitForDistance(0).GetDistance("miles"));
  }

  TransitDto TransitForDistance(float km)
  {
    var t = new Transit
    {
      Price = new Money(10),
      DateTime = SystemClock.Instance.InUtc().GetCurrentInstant(),
      To = new Address(),
      From = new Address(),
      Status = Transit.Statuses.Draft,
      Km = km,
      Client = new Client()
    };
    return new TransitDto(t);
  }
}