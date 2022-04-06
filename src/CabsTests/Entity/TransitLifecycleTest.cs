using System;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using NodaTime;

namespace LegacyFighter.CabsTests.Entity;

public class TransitLifeCycleTest
{
  public const long DriverId = 1L;
  public const long SecondDriverId = 2L;

  [Test]
  public void CanCreateTransit()
  {
    //when
    var transit = RequestTransit();

    //then
    Assert.Null(transit.Price);
    Assert.AreEqual(Transit.Statuses.Draft, transit.Status);
    Assert.NotNull(transit.Tariff);
    Assert.AreNotEqual(0, transit.Tariff.KmRate);
  }

  [Test]
  public void CanChangeTransitDestination()
  {
    //given
    var transit = RequestTransit();
    //when
    transit.ChangeDestinationTo(
      new Address("Polska", "Warszawa", "Mazowiecka", 30), Distance.OfKm(20));

    //then
    Assert.NotNull(transit.EstimatedPrice);
    Assert.Null(transit.Price);
  }

  [Test]
  public void CannotChangeDestinationWhenTransitIsCompleted()
  {
    //given
    var destination = new Address("Polska", "Warszawa", "Żytnia", 25);
    //and
    var transit = RequestTransit();
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(DriverId);
    //and
    transit.AcceptBy(DriverId, Now());
    //and
    transit.Start(Now());
    //and
    transit.CompleteTransitAt(Now(), destination, Distance.OfKm(20));

    //expect
    transit.Invoking(t => t.ChangeDestinationTo(
        new Address("Polska", "Warszawa", "Żytnia", 23), Distance.OfKm(20)))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void CanChangePickupPlace()
  {
    //given
    var transit = RequestTransit();

    //expect
    transit.Invoking(t => t.ChangePickupTo(
      new Address("Polska", "Warszawa", "Puławska", 28), Distance.OfKm(20), 0.2))
      .Should().NotThrow();
  }

  [Test]
  public void CannotChangePickupPlaceAfterTransitIsAccepted()
  {
    //given
    var destination = new Address("Polska", "Warszawa", "Żytnia", 25);
    //and
    var transit = RequestTransit();
    //and
    var changedTo = new Address("Polska", "Warszawa", "Żytnia", 27);
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(DriverId);
    //and
    transit.AcceptBy(DriverId, Now());

    //expect
    transit.Invoking(t => t.ChangePickupTo(changedTo, Distance.OfKm(20.1f), 0.1))
      .Should().ThrowExactly<InvalidOperationException>();

    //and
    transit.Start(Now());
    //expect
    transit.Invoking(t => t.ChangePickupTo(changedTo, Distance.OfKm(20.11f), 0.11))
      .Should().ThrowExactly<InvalidOperationException>();

    //and
    transit.CompleteTransitAt(Now(), destination, Distance.OfKm(20));
    //expect
    transit.Invoking(t => t.ChangePickupTo(changedTo, Distance.OfKm(20.12f), 0.12))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void CannotChangePickupPlaceMoreThanThreeTimes()
  {
    //given
    var transit = RequestTransit();
    //and
    transit.ChangePickupTo(
      new Address("Polska", "Warszawa", "Żytnia", 26), Distance.OfKm(20.1f), 0.1d);
    //and
    transit.ChangePickupTo(
      new Address("Polska", "Warszawa", "Żytnia", 27), Distance.OfKm(20.2f), 0.2d);
    //and
    transit.ChangePickupTo(
      new Address("Polska", "Warszawa", "Żytnia", 28), Distance.OfKm(20.22f), 0.22d);

    //expect
    transit.Invoking(t => t.ChangePickupTo(
        new Address("Polska", "Warszawa", "Żytnia", 29), Distance.OfKm(20.3f), 0.23d))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void CannotChangePickupPlaceWhenItIsFarWayFromOriginal()
  {
    //given
    var transit = RequestTransit();

    //expect
    transit.Invoking(t => t.ChangePickupTo(new Address(), Distance.OfKm(20), 50))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void CanCancelTransit()
  {
    //given
    var transit = RequestTransit();

    //when
    transit.Cancel();

    //then
    Assert.AreEqual(Transit.Statuses.Cancelled, transit.Status);
  }

  [Test]
  public void CannotCancelTransitAfterItWasStarted()
  {
    //given
    var destination =
      new Address("Polska", "Warszawa", "Żytnia", 25);
    //and
    var transit = RequestTransit();
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(DriverId);
    //and
    transit.AcceptBy(DriverId, Now());

    //and
    transit.Start(Now());
    //expect
    transit.Invoking(t => t.Cancel())
      .Should().ThrowExactly<InvalidOperationException>();

    //and
    transit.CompleteTransitAt(Now(), destination, Distance.OfKm(20));
    //expect
    transit.Invoking(t => t.Cancel())
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void CanPublishTransit()
  {
    //given
    var transit = RequestTransit();

    //when
    transit.PublishAt(Now());

    //then
    Assert.AreEqual(Transit.Statuses.WaitingForDriverAssignment, transit.Status);
    Assert.NotNull(transit.Published);
  }

  [Test]
  public void CanAcceptTransit()
  {
    //given
    var transit = RequestTransit();
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(DriverId);

    //when
    transit.AcceptBy(DriverId, Now());
    //then
    Assert.AreEqual(Transit.Statuses.TransitToPassenger, transit.Status);
  }

  [Test]
  public void OnlyOneDriverCanAcceptTransit()
  {
    //given
    var transit = RequestTransit();
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(DriverId);
    //and
    transit.AcceptBy(DriverId, Now());

    //expect
    transit.Invoking(t => t.AcceptBy(SecondDriverId, Now()))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void TransitCannotByAcceptedByDriverWhoAlreadyRejected()
  {
    //given
    var transit = RequestTransit();
    //and
    transit.PublishAt(Now());
    //and
    transit.RejectBy(DriverId);

    //expect
    transit.Invoking(t => t.AcceptBy(DriverId, Now()))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void TransitCannotByAcceptedByDriverWhoHasNotSeenProposal()
  {
    //given
    var transit = RequestTransit();
    //and
    transit.PublishAt(Now());

    //expect
    transit.Invoking(t => t.AcceptBy(DriverId, Now()))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void CanStartTransit()
  {
    //given
    var transit = RequestTransit();
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(DriverId);
    //and
    transit.AcceptBy(DriverId, Now());
    //when
    transit.Start(Now());

    //then
    Assert.AreEqual(Transit.Statuses.InTransit, transit.Status);
  }

  [Test]
  public void CannotStartNotAcceptedTransit()
  {
    //given
    var transit = RequestTransit();
    //and
    transit.PublishAt(Now());

    //expect
    transit.Invoking(t => t.Start(Now()))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void CanCompleteTransit()
  {
    //given
    var destination =
      new Address("Polska", "Warszawa", "Żytnia", 25);
    //and
    var transit = RequestTransit();
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(DriverId);
    //and
    transit.AcceptBy(DriverId, Now());
    //and
    transit.Start(Now());

    //when
    transit.CompleteTransitAt(Now(), destination, Distance.OfKm(20));

    //then
    Assert.AreEqual(Transit.Statuses.Completed, transit.Status);
    Assert.NotNull(transit.Tariff);
    Assert.NotNull(transit.Price);
  }

  [Test]
  public void CannotCompleteNotStartedTransit()
  {
    //given
    var addressTo =
      new Address("Polska", "Warszawa", "Żytnia", 25);
    //and
    var transit = RequestTransit();
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(DriverId);
    //and
    transit.AcceptBy(DriverId, Now());

    //expect
    transit.Invoking(t => t.CompleteTransitAt(Now(), addressTo, Distance.OfKm(20)))
      .Should().ThrowExactly<ArgumentException>();
  }

  [Test]
  public void CanRejectTransit()
  {
    //given
    var transit = RequestTransit();
    //and
    transit.PublishAt(Now());

    //when
    transit.RejectBy(DriverId);

    //then
    Assert.AreEqual(Transit.Statuses.WaitingForDriverAssignment, transit.Status);
  }

  private static Instant Now()
  {
    return SystemClock.Instance.GetCurrentInstant();
  }

  private static Transit RequestTransit()
  {
    return new Transit(Now(), Distance.Zero);
  }
}