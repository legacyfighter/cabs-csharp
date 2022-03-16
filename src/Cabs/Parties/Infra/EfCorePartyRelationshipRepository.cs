using Core.Maybe;
using LegacyFighter.Cabs.Parties.Api;
using LegacyFighter.Cabs.Parties.Model.Parties;
using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Parties.Infra;

public class EfCorePartyRelationshipRepository : IPartyRelationshipRepository
{
  private readonly SqLiteDbContext _dbContext;

  public EfCorePartyRelationshipRepository(SqLiteDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<PartyRelationship> Save(
    string partyRelationship,
    string partyARole,
    Party partyA,
    string partyBRole,
    Party partyB)
  {
    var parties = _dbContext.PartyRelationships.FromSqlInterpolated(
        $"SELECT * FROM PartyRelationships r WHERE r.Name = {partyRelationship} AND ((r.PartyAId = {partyA.Id} AND r.PartyBId = {partyB.Id}) OR (r.PartyAId = {partyB.Id} AND r.PartyBId = {partyA.Id}))");

    PartyRelationship relationship;
    if (await parties.CountAsync() == 0)
    {
      relationship = new PartyRelationship();
      await _dbContext.PartyRelationships.AddAsync(relationship);
      await _dbContext.SaveChangesAsync();
    }
    else
    {
      relationship = await parties.FirstAsync();
    }

    relationship.Name = partyRelationship;
    relationship.PartyA = partyA;
    relationship.PartyB = partyB;
    relationship.RoleA = partyARole;
    relationship.RoleB = partyBRole;

    return relationship;
  }

  public async Task<Maybe<PartyRelationship>> FindRelationshipFor(PartyId id, string relationshipName)
  {
    var parties = _dbContext.PartyRelationships.FromSqlInterpolated(
        $"SELECT * FROM PartyRelationships r WHERE r.Name = {relationshipName} AND (r.PartyAId = {id.ToGuid()} OR r.PartyBId = {id.ToGuid()})");
    if (await parties.CountAsync() == 0)
    {
      return Maybe<PartyRelationship>.Nothing;
    }
    return await parties.FirstAsync().JustAsync();
  }
}