using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Parties.Model.Parties;
using LegacyFighter.Cabs.Repair.Api;

namespace LegacyFighter.Cabs.Repair.Model.Roles.Repair;

public class Warranty : RoleForRepairer
{
  public Warranty(Party party) : base(party)
  {
  }

  public override RepairingResult Handle(RepairRequest repairRequest)
  {
    var handledParts = repairRequest.PartsToRepair.ToHashSet();

    return new RepairingResult(Party.Id, Money.Zero, handledParts);
  }
}