namespace LegacyFighter.Cabs.Parties.Model.Parties;

public interface IPartyRepository
{
  Task<Party> Save(Guid id);
}