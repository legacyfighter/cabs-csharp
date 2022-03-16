using System.Reflection;
using Core.Maybe;
using LegacyFighter.Cabs.Parties.Model.Parties;
using LegacyFighter.Cabs.Parties.Model.Role;
using LegacyFighter.Cabs.Parties.Utils;

namespace LegacyFighter.Cabs.Parties.Api;

/// <summary>
/// Sample impl based on Class-Instance map.
/// More advanced impls can be case on a DI container: GetRole can obtain role instance from the container.
///
/// TODO introduce an interface to convert to Abstract Factory Pattern to be able to choose factory impl
/// </summary>
public class RoleObjectFactory
{
  private readonly PolymorphicDictionary<PartyBasedRole> _roles = new();

  public bool HasRole<T>() where T : PartyBasedRole
  {
    return _roles.ContainsKey(typeof(T));
  }

  public static RoleObjectFactory From(PartyRelationship partyRelationship)
  {
    var roleObject = new RoleObjectFactory();
    roleObject.Add(partyRelationship);
    return roleObject;
  }

  private void Add(PartyRelationship partyRelationship)
  {
    Add(partyRelationship.RoleA, partyRelationship.PartyA);
    Add(partyRelationship.RoleB, partyRelationship.PartyB);
  }

  private void Add(string role, Party party)
  {
    try
    {
      //in sake of simplicity: a role name is same as a class name with no mapping between them
      var type = Assembly.GetAssembly(GetType()).DefinedTypes.Single(t => t.Name == role);
      var instance = (PartyBasedRole)Activator.CreateInstance(type, party);
      _roles[type] = instance;
    }
    catch (Exception e)
    {
      throw new ArgumentException("invalid role", nameof(role), e);
    }
  }

  public Maybe<T> GetRole<T>() where T : PartyBasedRole
  {
    return _roles[typeof(T)].ToMaybe().Cast<PartyBasedRole, T>();
  }
}