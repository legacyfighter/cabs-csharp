using System;
using LegacyFighter.Cabs.Loyalty;
using NodaTime;

namespace LegacyFighter.CabsTests.Loyalty;

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

  [Test]
  public void CannotSubtractFromTwoStepExpiringMiles()
  {
    //given
    IMiles expiringInTwoStepsMiles = new TwoStepExpiringMiles(10, Yesterday, Today);

    //expect
    expiringInTwoStepsMiles.Invoking(m => m.Subtract(11, Yesterday)).Should().ThrowExactly<ArgumentException>();
    expiringInTwoStepsMiles.Invoking(m => m.Subtract(11, Today)).Should().ThrowExactly<ArgumentException>();
    expiringInTwoStepsMiles.Invoking(m => m.Subtract(11, Tomorrow)).Should().ThrowExactly<ArgumentException>();
    expiringInTwoStepsMiles.Invoking(m => m.Subtract(2, Tomorrow)).Should().ThrowExactly<ArgumentException>();

  }

  [Test]
  public void TwoStepExpiringMilesShouldLeaveHalfOfAmountAfterOneStep()
  {
    //given
    IMiles twoStepExpiring = new TwoStepExpiringMiles(10, Yesterday, Today);

    //expect
    Assert.AreEqual(10, twoStepExpiring.GetAmountFor(Yesterday));
    Assert.AreEqual(5, twoStepExpiring.GetAmountFor(Today));
    Assert.AreEqual(0, twoStepExpiring.GetAmountFor(Tomorrow));

  }

  [Test]
  public void CanSubtractFromTwoStepExpiringMilesWhenEnoughMiles()
  {
    //given
    IMiles twoStepExpiringOdd = new TwoStepExpiringMiles(9, Yesterday, Today);
    IMiles twoStepExpiringEven = new TwoStepExpiringMiles(10, Yesterday, Today);

    //expect
    Assert.AreEqual(new TwoStepExpiringMiles(4, Yesterday, Today), twoStepExpiringOdd.Subtract(5, Yesterday));
    Assert.AreEqual(new TwoStepExpiringMiles(1, Yesterday, Today), twoStepExpiringOdd.Subtract(4, Today));

    Assert.AreEqual(new TwoStepExpiringMiles(5, Yesterday, Today), twoStepExpiringEven.Subtract(5, Yesterday));
    Assert.AreEqual(new TwoStepExpiringMiles(0, Yesterday, Today), twoStepExpiringEven.Subtract(5, Today));
  }

  [Test]
  public void TwoStepMilesCanBeSerializedAndDeserializedBack()
  {
    var originalMiles = new TwoStepExpiringMiles(20, Instant.FromUnixTimeTicks(20), Instant.FromUnixTimeTicks(30));
    var serialized = MilesJsonMapper.Serialize(originalMiles);
    var deserialized = MilesJsonMapper.Deserialize(serialized);
    deserialized.Should().Be(originalMiles);
  }
}