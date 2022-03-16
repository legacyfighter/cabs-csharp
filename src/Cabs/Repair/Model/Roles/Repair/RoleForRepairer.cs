using LegacyFighter.Cabs.Parties.Model.Parties;
using LegacyFighter.Cabs.Parties.Model.Role;
using LegacyFighter.Cabs.Repair.Api;

namespace LegacyFighter.Cabs.Repair.Model.Roles.Repair;

/// <summary>
/// Base class for all commands that are able to handle <see cref="RepairRequest"/>
/// </summary>
public abstract class RoleForRepairer : PartyBasedRole
{
  public RoleForRepairer(Party party) : base(party)
  {
  }

  public abstract RepairingResult Handle(RepairRequest repairRequest);
}