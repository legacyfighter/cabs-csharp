using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Repair.Legacy.Parts;

namespace LegacyFighter.Cabs.Repair.Model.Roles.Repair;

public class RepairingResult
{
  public RepairingResult(Guid handlingParty, Money totalCost, ISet<Part> handledParts)
  {
    HandlingParty = handlingParty;
    TotalCost = totalCost;
    HandledParts = handledParts;
  }

  public Guid HandlingParty { get; }
  public Money TotalCost { get; }
  public ISet<Part> HandledParts { get; }
}