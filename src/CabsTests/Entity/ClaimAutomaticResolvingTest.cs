using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;

namespace LegacyFighter.CabsTests.Entity;

public class ClaimAutomaticResolvingTest
{
  [Test]
  public void SecondClaimForTheSameTransitWillBeEscalated()
  {
    //given
    var resolver = new ClaimsResolver();
    //and
    var transit = ATransit(1L, 39);
    //and
    var claim = CreateClaim(transit);
    //and
    resolver.Resolve(claim, 40, 15, 10);
    //and
    var claim2 = CreateClaim(transit);

    //when
    var result = resolver.Resolve(claim2, 40, 15, 10);

    //then
    Assert.AreEqual(Claim.Statuses.Escalated, result.Decision);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskNoOne, result.WhoToAsk);
  }

  [Test]
  public void LowCostTransitsAreRefundedIfClientIsVip()
  {
    //given
    var resolver = new ClaimsResolver();
    //and
    var transit = ATransit(1L, 39);
    //and
    var claim = CreateClaim(transit);

    //when
    var result = resolver.Resolve(claim, 40, 15, 10);

    //then
    Assert.AreEqual(Claim.Statuses.Refunded, result.Decision);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskNoOne, result.WhoToAsk);
  }

  [Test]
  public void HighCostTransitsAreEscalatedEvenWhenClientIsVip()
  {
    //given
    var resolver = new ClaimsResolver();
    //and
    var claim = CreateClaim(ATransit(1L, 39));
    resolver.Resolve(claim, 40, 15, 10);
    var claim2 = CreateClaim(ATransit(2L, 39));
    resolver.Resolve(claim2, 40, 15, 10);
    var claim3 = CreateClaim(ATransit(3L, 39));
    resolver.Resolve(claim3, 40, 15, 10);
    //and
    var claim4 = CreateClaim(ATransit(4L, 41), AClient(Client.Types.Vip));

    //when
    var result = resolver.Resolve(claim4, 40, 15, 10);

    //then
    Assert.AreEqual(Claim.Statuses.Escalated, result.Decision);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskDriver, result.WhoToAsk);
  }

  [Test]
  public void FirstThreeClaimsAreRefunded()
  {
    //given
    var resolver = new ClaimsResolver();
    //and
    var claim = CreateClaim(ATransit(1L, 39));
    var result1 = resolver.Resolve(claim, 40, 15, 10);
    var claim2 = CreateClaim(ATransit(2L, 39));
    var result2 = resolver.Resolve(claim2, 40, 15, 10);
    var claim3 = CreateClaim(ATransit(3L, 39));
    var result3 = resolver.Resolve(claim3, 40, 15, 10);

    //when
    var claim4 = CreateClaim(ATransit(4L, 39), AClient(Client.Types.Normal));
    var result4 = resolver.Resolve(claim4, 40, 4, 10);

    //then
    Assert.AreEqual(Claim.Statuses.Refunded, result1.Decision);
    Assert.AreEqual(Claim.Statuses.Refunded, result2.Decision);
    Assert.AreEqual(Claim.Statuses.Refunded, result3.Decision);
    Assert.AreEqual(Claim.Statuses.Escalated, result4.Decision);

    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskNoOne, result1.WhoToAsk);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskNoOne, result2.WhoToAsk);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskNoOne, result3.WhoToAsk);
  }

  [Test]
  public void LowCostTransitsAreRefundedWhenManyTransits()
  {
    //given
    var resolver = new ClaimsResolver();
    //and
    var claim = CreateClaim(ATransit(1L, 39));
    resolver.Resolve(claim, 40, 15, 10);
    var claim2 = CreateClaim(ATransit(2L, 39));
    resolver.Resolve(claim2, 40, 15, 10);
    var claim3 = CreateClaim(ATransit(3L, 39));
    resolver.Resolve(claim3, 40, 15, 10);
    //and
    var claim4 = CreateClaim(ATransit(4L, 39), AClient(Client.Types.Normal));

    //when
    var result = resolver.Resolve(claim4, 40, 10, 9);

    //then
    Assert.AreEqual(Claim.Statuses.Refunded, result.Decision);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskNoOne, result.WhoToAsk);
  }

  [Test]
  public void HighCostTransitsAreEscalatedEvenWithManyTransits()
  {
    //given
    var resolver = new ClaimsResolver();
    //and
    var claim = CreateClaim(ATransit(1L, 39));
    resolver.Resolve(claim, 40, 15, 10);
    var claim2 = CreateClaim(ATransit(2L, 39));
    resolver.Resolve(claim2, 40, 15, 10);
    var claim3 = CreateClaim(ATransit(3L, 39));
    resolver.Resolve(claim3, 40, 15, 10);
    //and
    var claim4 = CreateClaim(ATransit(4L, 50), AClient(Client.Types.Normal));

    //when
    var result = resolver.Resolve(claim4, 40, 12, 10);

    //then
    Assert.AreEqual(Claim.Statuses.Escalated, result.Decision);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskClient, result.WhoToAsk);
  }

  [Test]
  public void HighCostTransitsAreEscalatedWhenFewTransits()
  {
    //given
    var resolver = new ClaimsResolver();
    //and
    var claim = CreateClaim(ATransit(1L, 39));
    resolver.Resolve(claim, 40, 15, 10);
    var claim2 = CreateClaim(ATransit(2L, 39));
    resolver.Resolve(claim2, 40, 15, 10);
    var claim3 = CreateClaim(ATransit(3L, 39));
    resolver.Resolve(claim3, 40, 15, 10);
    //and
    var claim4 = CreateClaim(ATransit(4L, 50), AClient(Client.Types.Normal));

    //when
    var result = resolver.Resolve(claim4, 40, 2, 10);

    //then
    Assert.AreEqual(Claim.Statuses.Escalated, result.Decision);
    Assert.AreEqual(ClaimsResolver.WhoToAsk.AskDriver, result.WhoToAsk);
  }

  private Transit ATransit(long? id, int price)
  {
    var transit = new Transit(id)
    {
      Price = new Money(price)
    };
    return transit;
  }

  private Claim CreateClaim(Transit transit)
  {
    var claim = new Claim
    {
      TransitId = transit.Id,
      TransitPrice = transit.Price
    };
    return claim;
  }

  private Claim CreateClaim(Transit transit, Client client)
  {
    var claim = new Claim
    {
      TransitId = transit.Id,
      TransitPrice = transit.Price,
      Owner = client
    };
    return claim;
  }

  private Client AClient(Client.Types type)
  {
    var client = new Client
    {
      Type = type
    };
    return client;
  }
}