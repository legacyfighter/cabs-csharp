using LegacyFighter.Cabs.Parties.Model.Parties;
using LegacyFighter.Cabs.Parties.Model.Role;
using LegacyFighter.Cabs.Repair.Api;

namespace LegacyFighter.Cabs.Repair.Model.Roles.Assistance;

/// <summary>
/// Base class for all commands that are able to handle <see cref="AssistanceRequest"/>
/// </summary>
public abstract class RoleForAssistance : PartyBasedRole
{
  public RoleForAssistance(Party party) : base(party)
  {
  }

  public abstract void Handle(AssistanceRequest request);
}