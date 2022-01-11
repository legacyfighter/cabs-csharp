using LegacyFighter.Cabs.MoneyValue;

namespace LegacyFighter.CabsTests.MoneyValue;

public class MoneyTest
{
  [Test]
  public void CanCreateMoneyFromInteger()
  {
    //expect
    Assert.AreEqual("100.00", new Money(10000).ToString());
    Assert.AreEqual("0.00", new Money(0).ToString());
    Assert.AreEqual("10.12", new Money(1012).ToString());
  }

  [Test]
  public void ShouldProjectMoneyToInteger()
  {
    //expect
    Assert.AreEqual(10, new Money(10).ToInt());
    Assert.AreEqual(0, new Money(0).ToInt());
    Assert.AreEqual(-5, new Money(-5).ToInt());
  }

  [Test]
  public void CanAddMoney()
  {
    //expect
    Assert.AreEqual(new Money(1000), new Money(500) + new Money(500));
    Assert.AreEqual(new Money(1042), new Money(1020) + new Money(22));
    Assert.AreEqual(new Money(0), new Money(0) + new Money(0));
    Assert.AreEqual(new Money(-2), new Money(-4) + new Money(2));
  }

  [Test]
  public void CanSubtractMoney()
  {
    //expect
    Assert.AreEqual(Money.Zero, new Money(50) - new Money(50));
    Assert.AreEqual(new Money(998), new Money(1020) - new Money(22));
    Assert.AreEqual(new Money(-1), new Money(2) - new Money(3));
  }

  [Test]
  public void CanCalculatePercentage()
  {
    //expect
    Assert.AreEqual("30.00", new Money(10000).Percentage(30).ToString());
    Assert.AreEqual("26.40", new Money(8800).Percentage(30).ToString());
    Assert.AreEqual("88.00", new Money(8800).Percentage(100).ToString());
    Assert.AreEqual("0.00", new Money(8800).Percentage(0).ToString());
    Assert.AreEqual("13.20", new Money(4400).Percentage(30).ToString());
    Assert.AreEqual("0.30", new Money(100).Percentage(30).ToString());
    Assert.AreEqual("0.00", new Money(1).Percentage(40).ToString());
  }
}