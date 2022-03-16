using Core.Maybe;
using LegacyFighter.Cabs.Parties.Model.Parties;

namespace LegacyFighter.Cabs.Parties.Api;

public class PartyMapper
{
  private readonly IPartyRelationshipRepository _partyRelationshipRepository;

  public PartyMapper(IPartyRelationshipRepository partyRelationshipRepository)
  {
    _partyRelationshipRepository = partyRelationshipRepository;
  }

  public async Task<Maybe<PartyRelationship>> MapRelation(PartyId id, string relationshipName)
  {
    return await _partyRelationshipRepository.FindRelationshipFor(id, relationshipName);
  }
}