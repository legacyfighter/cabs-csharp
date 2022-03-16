using LegacyFighter.Cabs.Parties.Model.Parties;
using LegacyFighter.Cabs.Parties.Model.Role;

namespace LegacyFighter.Cabs.Repair.Model.Roles.Empty;

public class Customer : PartyBasedRole
{
  public Customer(Party party) : base(party)
  {
  }
}