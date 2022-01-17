using System;
using LegacyFighter.Cabs.Entity.Miles;
using NodaTime;

namespace LegacyFighter.CabsTests.Entity.Miles;

public class MilesTest
{
  private static readonly Instant Yesterday = new LocalDateTime(1989, 12, 12, 12, 12).InUtc().ToInstant();
  private static readonly Instant Today = Yesterday.Plus(Duration.FromDays(1));
  private static readonly Instant Tomorrow = Today.Plus(Duration.FromDays(1));

  [Test]
  public void MilesWithoutExpirationDateDontExpire()
  {
    //given
    var neverExpiring = ConstantUntil.Forever(10);

    //expect
    Assert.AreEqual(10, neverExpiring.GetAmountFor(Yesterday));
    Assert.AreEqual(10, neverExpiring.GetAmountFor(Today));
    Assert.AreEqual(10, neverExpiring.GetAmountFor(Tomorrow));
  }

  [Test]
  public void ExpiringMilesExpire()
  {
    //given
    var expiringMiles = ConstantUntil.Value(10, Today);

    //expect
    Assert.AreEqual(10, expiringMiles.GetAmountFor(Yesterday));
    Assert.AreEqual(10, expiringMiles.GetAmountFor(Today));
    Assert.AreEqual(0, expiringMiles.GetAmountFor(Tomorrow));
  }

  [Test]
  public void CanSubtractWhenEnoughMiles()
  {
    //given
    var expiringMiles = ConstantUntil.Value(10, Today);
    var neverExpiring = ConstantUntil.Forever(10);

    //expect
    Assert.AreEqual(ConstantUntil.Value(0, Today), expiringMiles.Subtract(10, Today));
    Assert.AreEqual(ConstantUntil.Value(0, Today), expiringMiles.Subtract(10, Yesterday));

    Assert.AreEqual(ConstantUntil.Value(2, Today), expiringMiles.Subtract(8, Today));
    Assert.AreEqual(ConstantUntil.Value(2, Today), expiringMiles.Subtract(8, Yesterday));

    Assert.AreEqual(ConstantUntil.Forever(0), neverExpiring.Subtract(10, Yesterday));
    Assert.AreEqual(ConstantUntil.Forever(0), neverExpiring.Subtract(10, Today));
    Assert.AreEqual(ConstantUntil.Forever(0), neverExpiring.Subtract(10, Tomorrow));

    Assert.AreEqual(ConstantUntil.Forever(2), neverExpiring.Subtract(8, Yesterday));
    Assert.AreEqual(ConstantUntil.Forever(2), neverExpiring.Subtract(8, Today));
    Assert.AreEqual(ConstantUntil.Forever(2), neverExpiring.Subtract(8, Tomorrow));
  }

  [Test]
  public void CannotSubtractWhenNotEnoughMiles()
  {
    //given
    var neverExpiring = ConstantUntil.Forever(10);
    var expiringMiles = ConstantUntil.Value(10, Today);

    //expect
    neverExpiring.Invoking(m => m.Subtract(11, Yesterday)).Should().ThrowExactly<ArgumentException>();
    neverExpiring.Invoking(m => m.Subtract(11, Today)).Should().ThrowExactly<ArgumentException>();
    neverExpiring.Invoking(m => m.Subtract(11, Tomorrow)).Should().ThrowExactly<ArgumentException>();

    expiringMiles.Invoking(m => m.Subtract(11, Yesterday)).Should().ThrowExactly<ArgumentException>();
    expiringMiles.Invoking(m => m.Subtract(11, Today)).Should().ThrowExactly<ArgumentException>();
    expiringMiles.Invoking(m => m.Subtract(8, Tomorrow)).Should().ThrowExactly<ArgumentException>();
    expiringMiles.Invoking(m => m.Subtract(8, Tomorrow)).Should().ThrowExactly<ArgumentException>();
  }
}