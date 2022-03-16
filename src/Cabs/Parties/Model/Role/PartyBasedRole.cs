using LegacyFighter.Cabs.Parties.Model.Parties;

namespace LegacyFighter.Cabs.Parties.Model.Role;

/// <summary>
/// TODO introduce interface for an abstract class
/// </summary>
public abstract class PartyBasedRole
{
  protected Party Party;

  public PartyBasedRole(Party party)
  {
    Party = party;
  }
}