using LegacyFighter.Cabs.Parties.Model.Parties;
using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.Parties.Infra;

public class EfCorePartyRepository : IPartyRepository
{
  private readonly SqLiteDbContext _dbContext;

  public EfCorePartyRepository(SqLiteDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<Party> Save(Guid id)
  {
    var party = await _dbContext.Parties.FindAsync(id);
    if (party == null)
    {
      party = new Party
      {
        Id = id
      };
      await _dbContext.Parties.AddAsync(party);
      await _dbContext.SaveChangesAsync();
    }

    return party;
  }
}