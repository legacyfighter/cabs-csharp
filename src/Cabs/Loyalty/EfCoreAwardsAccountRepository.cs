using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Loyalty;

public interface IAwardsAccountRepository
{
  Task<AwardsAccount> FindByClientId(long? clientId);
  Task<AwardsAccount> Save(AwardsAccount account);
  Task<IReadOnlyList<AwardedMiles>> FindAllMilesBy(Client client);
}

internal class EfCoreAwardsAccountRepository : IAwardsAccountRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreAwardsAccountRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<AwardsAccount> FindByClientId(long? clientId)
  {
    return await _context.AwardsAccounts.FirstOrDefaultAsync(a => a.ClientId == clientId);
  }

  public async Task<AwardsAccount> Save(AwardsAccount account)
  {
    _context.AwardsAccounts.Update(account);
    await _context.SaveChangesAsync();
    return account;
  }

  public async Task<IReadOnlyList<AwardedMiles>> FindAllMilesBy(Client client) 
  {
    return (await FindByClientId(client.Id)).GetMiles();
  }
}