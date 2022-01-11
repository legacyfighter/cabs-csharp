using System;
using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.Entity;
using NodaTime;

namespace LegacyFighter.CabsTests.Entity;

public class TransitLifeCycleTest
{
  [Test]
  public void CanCreateTransit()
  {
    //when
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      new Address("Polska", "Warszawa", "Żytnia", 25));

    //then
    Assert.Null(transit.CarType);
    Assert.Null(transit.Price);
    Assert.AreEqual("Polska", transit.From.Country);
    Assert.AreEqual("Warszawa", transit.From.City);
    Assert.AreEqual("Młynarska", transit.From.Street);
    Assert.AreEqual(20, transit.From.BuildingNumber);
    Assert.AreEqual("Polska", transit.To.Country);
    Assert.AreEqual("Warszawa", transit.To.City);
    Assert.AreEqual("Żytnia", transit.To.Street);
    Assert.AreEqual(25, transit.To.BuildingNumber);
    Assert.AreEqual(Transit.Statuses.Draft, transit.Status);
    Assert.NotNull(transit.Tariff);
    Assert.AreNotEqual(0, transit.Tariff.KmRate);
    Assert.NotNull(transit.DateTime);
  }

  [Test]
  public void CanChangeTransitDestination()
  {
    //given
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      new Address("Polska", "Warszawa", "Żytnia", 25));
    //when
    transit.ChangeDestinationTo(
      new Address("Polska", "Warszawa", "Mazowiecka", 30), Distance.OfKm(20));

    //then
    Assert.AreEqual(30, transit.To.BuildingNumber);
    Assert.AreEqual("Mazowiecka", transit.To.Street);
    Assert.NotNull(transit.EstimatedPrice);
    Assert.Null(transit.Price);
  }

  [Test]
  public void CannotChangeDestinationWhenTransitIsCompleted()
  {
    //given
    var destination = new Address("Polska", "Warszawa", "Żytnia", 25);
    //and
    var driver = new Driver();
    //and
    var transit = RequestTransitFromTo(new Address("Polska", "Warszawa", "Młynarska", 20),
      destination);
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(driver);
    //and
    transit.AcceptBy(driver, Now());
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
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      new Address("Polska", "Warszawa", "Żytnia", 25));

    //when
    transit.ChangePickupTo(
      new Address("Polska", "Warszawa", "Puławska", 28), Distance.OfKm(20), 0.2);

    //then
    Assert.AreEqual(28, transit.From.BuildingNumber);
    Assert.AreEqual("Puławska", transit.From.Street);
  }

  [Test]
  public void CannotChangePickupPlaceAfterTransitIsAccepted()
  {
    //given
    var destination = new Address("Polska", "Warszawa", "Żytnia", 25);
    //and
    var driver = new Driver();
    //and
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      destination);
    //and
    var changedTo = new Address("Polska", "Warszawa", "Żytnia", 27);
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(driver);
    //and
    transit.AcceptBy(driver, Now());

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
    var transit = RequestTransitFromTo(new Address("Polska", "Warszawa", "Młynarska", 20),
      new Address("Polska", "Warszawa", "Żytnia", 25));
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
    var transit = RequestTransitFromTo(new Address("Polska", "Warszawa", "Młynarska", 20),
      new Address("Polska", "Warszawa", "Żytnia", 25));

    //expect
    transit.Invoking(t => t.ChangePickupTo(new Address(), Distance.OfKm(20), 50))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void CanCancelTransit()
  {
    //given
    var transit = RequestTransitFromTo(new Address("Polska", "Warszawa", "Młynarska", 20),
      new Address("Polska", "Warszawa", "Żytnia", 25));

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
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      destination);
    //and
    var driver = new Driver();
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(driver);
    //and
    transit.AcceptBy(driver, Now());

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
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      new Address("Polska", "Warszawa", "Żytnia", 25));

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
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      new Address("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = new Driver();
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(driver);

    //when
    transit.AcceptBy(driver, Now());
    //then
    Assert.AreEqual(Transit.Statuses.TransitToPassenger, transit.Status);
    Assert.NotNull(transit.AcceptedAt);
  }

  [Test]
  public void onlyOneDriverCanAcceptTransit()
  {
    //given
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      new Address("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = new Driver();
    //and
    var secondDriver = new Driver();
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(driver);
    //and
    transit.AcceptBy(driver, Now());

    //expect
    transit.Invoking(t => t.AcceptBy(secondDriver, Now()))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void TransitCannotByAcceptedByDriverWhoAlreadyRejected()
  {
    //given
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      new Address("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = new Driver();
    //and
    transit.PublishAt(Now());
    //and
    transit.RejectBy(driver);

    //expect
    transit.Invoking(t => t.AcceptBy(driver, Now()))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void TransitCannotByAcceptedByDriverWhoHasNotSeenProposal()
  {
    //given
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      new Address("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = new Driver();
    //and
    transit.PublishAt(Now());

    //expect
    transit.Invoking(t => t.AcceptBy(driver, Now()))
      .Should().ThrowExactly<InvalidOperationException>();
  }

  [Test]
  public void CanStartTransit()
  {
    //given
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      new Address("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = new Driver();
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(driver);
    //and
    transit.AcceptBy(driver, Now());
    //when
    transit.Start(Now());

    //then
    Assert.AreEqual(Transit.Statuses.InTransit, transit.Status);
    Assert.NotNull(transit.Started);
  }

  [Test]
  public void CannotStartNotAcceptedTransit()
  {
    //given
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      new Address("Polska", "Warszawa", "Żytnia", 25));
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
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      destination);
    //and
    var driver = new Driver();
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(driver);
    //and
    transit.AcceptBy(driver, Now());
    //and
    transit.Start(Now());

    //when
    transit.CompleteTransitAt(Now(), destination, Distance.OfKm(20));

    //then
    Assert.AreEqual(Transit.Statuses.Completed, transit.Status);
    Assert.NotNull(transit.Tariff);
    Assert.NotNull(transit.Price);
    Assert.NotNull(transit.CompleteAt);
  }

  [Test]
  public void CannotCompleteNotStartedTransit()
  {
    //given
    var addressTo =
      new Address("Polska", "Warszawa", "Żytnia", 25);
    //and
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      addressTo);
    //and
    var driver = new Driver();
    //and
    transit.PublishAt(Now());
    //and
    transit.ProposeTo(driver);
    //and
    transit.AcceptBy(driver, Now());

    //expect
    transit.Invoking(t => t.CompleteTransitAt(Now(), addressTo, Distance.OfKm(20)))
      .Should().ThrowExactly<ArgumentException>();
  }

  [Test]
  public void CanRejectTransit()
  {
    //given
    var transit = RequestTransitFromTo(
      new Address("Polska", "Warszawa", "Młynarska", 20),
      new Address("Polska", "Warszawa", "Żytnia", 25));
    //and
    var driver = new Driver();
    //and
    transit.PublishAt(Now());

    //when
    transit.RejectBy(driver);

    //then
    Assert.AreEqual(Transit.Statuses.WaitingForDriverAssignment, transit.Status);
    Assert.Null(transit.AcceptedAt);
  }

  private static Instant Now()
  {
    return SystemClock.Instance.GetCurrentInstant();
  }

  private static Transit RequestTransitFromTo(Address pickup, Address destination)
  {
    return new Transit(pickup, destination, new Client(), null, Now(), Distance.Zero);
  }
}