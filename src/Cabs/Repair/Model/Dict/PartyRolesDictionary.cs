using LegacyFighter.Cabs.Parties.Model.Role;
using LegacyFighter.Cabs.Repair.Model.Roles.Empty;
using LegacyFighter.Cabs.Repair.Model.Roles.Repair;

namespace LegacyFighter.Cabs.Repair.Model.Dict;

/// <summary>
/// Classes that emulate database dictionary
/// </summary>
public static class PartyRolesDictionary
{
  public static PartyRolesDictionary<ExtendedInsurance> Insurer => new(nameof(Insurer));
  public static PartyRolesDictionary<Insured> Insured => new(nameof(Insured));
  public static PartyRolesDictionary<Warranty> Guarantor => new(nameof(Guarantor));
  public static PartyRolesDictionary<Customer> Customer => new(nameof(Customer));
}

public sealed record PartyRolesDictionary<T> where T : PartyBasedRole
{
  private readonly string _memberName;

  public PartyRolesDictionary(string memberName)
  {
    _memberName = memberName;
  }

  public string RoleName => typeof(T).Name;

  public override string ToString()
  {
    return _memberName;
  }
}