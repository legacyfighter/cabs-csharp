namespace LegacyFighter.Cabs.Parties.Api;

public record PartyId
{
  private readonly Guid _id;

  public PartyId()
  {
    _id = Guid.NewGuid();
  }

  public PartyId(Guid id)
  {
    _id = id;
  }

  public Guid ToGuid()
  {
    return _id;
  }
}