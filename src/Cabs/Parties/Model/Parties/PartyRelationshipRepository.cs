using Core.Maybe;
using LegacyFighter.Cabs.Parties.Api;

namespace LegacyFighter.Cabs.Parties.Model.Parties;

public interface IPartyRelationshipRepository
{
  Task<PartyRelationship> Save(
    string partyRelationship,
    string partyARole,
    Party partyA,
    string partyBRole,
    Party partyB);

  Task<Maybe<PartyRelationship>> FindRelationshipFor(PartyId id, string relationshipName);
}