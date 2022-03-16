
namespace LegacyFighter.Cabs.Parties.Model.Parties;

public class PartyRelationship
{
  public Guid? Id { get; set; }
  public string Name { get; set; }
  public string RoleA { get; set; }
  public string RoleB { get; set; }
  public virtual Party PartyA { get; set; }
  public virtual Party PartyB { get; set; }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as PartyRelationship)?.Id;
  }

  public static bool operator ==(PartyRelationship left, PartyRelationship right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(PartyRelationship left, PartyRelationship right)
  {
    return !Equals(left, right);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(Id);
  }
}