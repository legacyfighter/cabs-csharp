using System.Collections.Generic;
using System.Linq;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Parties.Api;
using LegacyFighter.Cabs.Repair.Api;
using LegacyFighter.Cabs.Repair.Legacy.Parts;

namespace LegacyFighter.CabsTests.Repair.Api;

internal class VehicleRepairAssert
{
  private readonly ResolveResult _result;

  public VehicleRepairAssert(ResolveResult result) : this(result, true)
  {

  }

  public VehicleRepairAssert(ResolveResult result, bool demandSuccess)
  {
    _result = result;
    if (demandSuccess)
    {
      Assert.AreEqual(ResolveResult.Statuses.Success, result.Status);
    }
    else
    {
      Assert.AreEqual(ResolveResult.Statuses.Error, result.Status);
    }
  }

  public VehicleRepairAssert Free()
  {
    Assert.AreEqual(Money.Zero, _result.TotalCost);
    return this;
  }

  public VehicleRepairAssert AllParts(ISet<Part> parts)
  {
    Assert.AreEqual(parts, _result.AcceptedParts);
    return this;
  }

  public VehicleRepairAssert By(PartyId handlingParty)
  {
    Assert.AreEqual(handlingParty.ToGuid(), _result.HandlingParty);
    return this;
  }

  public VehicleRepairAssert AllPartsBut(ISet<Part> parts, Part[] excludedParts)
  {
    var expectedParts = parts.ToHashSet();
    expectedParts.ExceptWith(excludedParts);

    Assert.AreEqual(expectedParts, _result.AcceptedParts);
    return this;
  }
}