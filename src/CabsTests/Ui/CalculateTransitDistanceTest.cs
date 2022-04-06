using System;
using System.Collections.Generic;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.TransitDetail;
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

  private TransitDto TransitForDistance(float km)
  {
    var tariff = Tariff.OfTime(LocalDateTimeNow());
    var transitDetails = new TransitDetailsDto(1L,
      Now(),
      Now(),
      new ClientDto(),
      null,
      new AddressDto(),
      new AddressDto(),
      Now(),
      Now(),
      Distance.OfKm(km),
      tariff);
    return new TransitDto(
      transitDetails,
      new HashSet<DriverDto>(),
      new HashSet<DriverDto>(),
      null);
  }

  private static Instant Now()
  {
    return SystemClock.Instance.GetCurrentInstant();
  }

  private static LocalDateTime LocalDateTimeNow()
  {
    return SystemClock.Instance.InBclSystemDefaultZone().GetCurrentLocalDateTime();
  }
}