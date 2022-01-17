using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Entity.Miles;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Repository;

public interface IAwardsAccountRepository
{
  Task<AwardsAccount> FindByClient(Client client);
  Task<AwardsAccount> Save(AwardsAccount account);
}

internal class EfCoreAwardsAccountRepository : IAwardsAccountRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreAwardsAccountRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<AwardsAccount> FindByClient(Client client)
  {
    return await _context.AwardsAccounts.FirstOrDefaultAsync(a => a.Client == client);
  }

  public async Task<AwardsAccount> Save(AwardsAccount account)
  {
    _context.AwardsAccounts.Update(account);
    await _context.SaveChangesAsync();
    return account;
  }
}