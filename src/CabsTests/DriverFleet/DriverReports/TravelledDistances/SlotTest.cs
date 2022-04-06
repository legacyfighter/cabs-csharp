using System;
using LegacyFighter.Cabs.DriverFleet.DriverReports.TravelledDistances;
using NodaTime;

namespace LegacyFighter.CabsTests.DriverFleet.DriverReports.TravelledDistances;

public class SlotTest
{
  private static readonly Instant Noon = new LocalDateTime(1989, 12, 12, 12, 10).InUtc().ToInstant();
  private static readonly Instant NoonFive = Noon.Plus(Duration.FromMinutes(5));
  private static readonly Instant NoonTen = NoonFive.Plus(Duration.FromMinutes(5));

  [Test]
  public void BeginningMustBeBeforeEnd()
  {
    //expect
    this.Invoking(_ => TimeSlot.Of(NoonFive, Noon)).Should().ThrowExactly<ArgumentException>();
    this.Invoking(_ => TimeSlot.Of(NoonTen, Noon)).Should().ThrowExactly<ArgumentException>();
    this.Invoking(_ => TimeSlot.Of(NoonTen, NoonFive)).Should().ThrowExactly<ArgumentException>();
    this.Invoking(_ => TimeSlot.Of(NoonTen, NoonTen)).Should().ThrowExactly<ArgumentException>();
  }

  [Test]
  public void CanCreateValidSlot()
  {
    //given
    var noonToFive = TimeSlot.Of(Noon, NoonFive);
    var fiveToTen = TimeSlot.Of(NoonFive, NoonTen);

    //expect
    Assert.AreEqual(Noon, noonToFive.Beginning);
    Assert.AreEqual(NoonFive, noonToFive.End);
    Assert.AreEqual(NoonFive, fiveToTen.Beginning);
    Assert.AreEqual(NoonTen, fiveToTen.End);
  }

  [Test]
  public void CanCreatePreviousSLot()
  {
    //given
    var noonToFive = TimeSlot.Of(Noon, NoonFive);
    var fiveToTen = TimeSlot.Of(NoonFive, NoonTen);
    var tenToFifteen = TimeSlot.Of(NoonTen, NoonTen.Plus(Duration.FromMinutes(5)));

    //expect
    Assert.AreEqual(noonToFive, fiveToTen.Prev());
    Assert.AreEqual(fiveToTen, tenToFifteen.Prev());
    Assert.AreEqual(noonToFive, tenToFifteen.Prev().Prev());
  }

  [Test]
  public void CanCalculateIfTimestampIsWithin()
  {
    //given
    var noonToFive = TimeSlot.Of(Noon, NoonFive);
    var fiveToTen = TimeSlot.Of(NoonFive, NoonTen);

    //expect
    Assert.True(noonToFive.Contains(Noon));
    Assert.True(noonToFive.Contains(Noon.Plus(Duration.FromMinutes(1))));
    Assert.False(noonToFive.Contains(NoonFive));
    Assert.False(noonToFive.Contains(NoonFive.Plus(Duration.FromMinutes(1))));

    Assert.False(noonToFive.IsBefore(Noon));
    Assert.False(noonToFive.IsBefore(NoonFive));
    Assert.True(noonToFive.IsBefore(NoonTen));

    Assert.True(noonToFive.EndsAt(NoonFive));

    Assert.False(fiveToTen.Contains(Noon));
    Assert.True(fiveToTen.Contains(NoonFive));
    Assert.True(fiveToTen.Contains(NoonFive.Plus(Duration.FromMinutes(1))));
    Assert.False(fiveToTen.Contains(NoonTen));
    Assert.False(fiveToTen.Contains(NoonTen.Plus(Duration.FromMinutes(1))));

    Assert.False(fiveToTen.IsBefore(Noon));
    Assert.False(fiveToTen.IsBefore(NoonFive));
    Assert.False(fiveToTen.IsBefore(NoonTen));
    Assert.True(fiveToTen.IsBefore(NoonTen.Plus(Duration.FromMinutes(1))));

    Assert.True(fiveToTen.EndsAt(NoonTen));
  }

  [Test]
  public void CanCreateSlotFromSeedWithinThatSlot()
  {
    //expect
    Assert.AreEqual(TimeSlot.Of(Noon, NoonFive), TimeSlot.ThatContains(Noon.Plus(Duration.FromMinutes(1))));
    Assert.AreEqual(TimeSlot.Of(Noon, NoonFive), TimeSlot.ThatContains(Noon.Plus(Duration.FromMinutes(2))));
    Assert.AreEqual(TimeSlot.Of(Noon, NoonFive), TimeSlot.ThatContains(Noon.Plus(Duration.FromMinutes(3))));
    Assert.AreEqual(TimeSlot.Of(Noon, NoonFive), TimeSlot.ThatContains(Noon.Plus(Duration.FromMinutes(4))));

    Assert.AreEqual(TimeSlot.Of(NoonFive, NoonTen), TimeSlot.ThatContains(NoonFive.Plus(Duration.FromMinutes(1))));
    Assert.AreEqual(TimeSlot.Of(NoonFive, NoonTen), TimeSlot.ThatContains(NoonFive.Plus(Duration.FromMinutes(2))));
    Assert.AreEqual(TimeSlot.Of(NoonFive, NoonTen), TimeSlot.ThatContains(NoonFive.Plus(Duration.FromMinutes(3))));
  }
}