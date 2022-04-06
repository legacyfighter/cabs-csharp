using System;
using LegacyFighter.Cabs.Ride;

namespace LegacyFighter.CabsTests.Ride;

public class TransitDemandTest
{
  [Test]
  public void CanChangePickupPlace()
  {
    //given
    var transitDemand = NewTransitDemand();

    //expect
    transitDemand.Invoking(t => t.ChangePickup(0.2))
      .Should().NotThrow();

  }

  [Test]
  public void CannotChangePickupPlaceAfterTransitIsAccepted()
  {
    //given
    var transitDemand = NewTransitDemand();
    //and
    transitDemand.Accepted();

    //expect
    transitDemand.Invoking(t => t.ChangePickup(0.1))
      .Should().ThrowExactly<InvalidOperationException>();
    //and
    //expect
    transitDemand.Invoking(t => t.ChangePickup(0.11))
      .Should().ThrowExactly<InvalidOperationException>();

  }

  [Test]
  public void CannotChangePickupPlaceMoreThanThreeTimes()
  {
    //given
    var transitDemand = NewTransitDemand();
    //and
    transitDemand.ChangePickup(0.1d);
    //and
    transitDemand.ChangePickup(0.2d);
    //and
    transitDemand.ChangePickup(0.22d);

    //expect
    transitDemand.Invoking(t => t.ChangePickup(0.23d))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void CannotChangePickupPlaceWhenItIsFarWayFromOriginal()
  {
    //given
    var transitDemand = NewTransitDemand();

    //expect
    transitDemand.Invoking(t => t.ChangePickup(50))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void CanCancelDemand()
  {
    //given
    var transitDemand = NewTransitDemand();

    //when
    transitDemand.Cancel();

    //then
    Assert.AreEqual(Cabs.Ride.TransitDemand.Statuses.Cancelled, transitDemand.Status);
  }

  [Test]
  public void CanPublishDemand()
  {
    //given
    var transitDemand = NewTransitDemand();

    //then
    Assert.AreEqual(TransitDemand.Statuses.WaitingForDriverAssignment, transitDemand.Status);
  }


  private TransitDemand NewTransitDemand()
  {
    return new TransitDemand(Guid.NewGuid());
  }
}