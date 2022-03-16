using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Parties.Model.Parties;
using LegacyFighter.Cabs.Repair.Api;
using LegacyFighter.Cabs.Repair.Legacy.Parts;

namespace LegacyFighter.Cabs.Repair.Model.Roles.Repair;

public class ExtendedInsurance : RoleForRepairer
{
  public ExtendedInsurance(Party party) : base(party)
  {
  }

  public override RepairingResult Handle(RepairRequest repairRequest)
  {
    var handledParts = repairRequest.PartsToRepair.ToHashSet();
    handledParts.Remove(Part.Paint);

    return new RepairingResult(Party.Id, Money.Zero, handledParts);
  }
}