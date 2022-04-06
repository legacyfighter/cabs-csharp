using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.Crm;

public interface IClientRepository
{
  Task<Client> Find(long? clientId);
  Task<Client> Save(Client client);
}

internal class EfCoreClientRepository : IClientRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreClientRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<Client> Find(long? clientId)
  {
    return await _context.Clients.FindAsync(clientId);
  }

  public async Task<Client> Save(Client client)
  {
    _context.Clients.Update(client);
    await _context.SaveChangesAsync();
    return client;
  }
}