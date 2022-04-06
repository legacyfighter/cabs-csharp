using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Geolocation.Address;

public interface IAddressRepositoryInterface
{
  Task<Address?> FindByHash(int hash);
  Task<Address> Find(long? id);
  Task<Address> Save(Address address);
  async Task<int?> FindHashById(long? id) => (await Find(id)).Hash;
}

internal class EfCoreAddressRepository : IAddressRepositoryInterface
{
  private readonly SqLiteDbContext _context;

  public EfCoreAddressRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<Address?> FindByHash(int hash)
  {
    return await _context.Addresses.FirstOrDefaultAsync(address => address.Hash == hash);
  }

  public async Task<Address> Find(long? id)
  {
    return await _context.Addresses.FindAsync(id);
  }

  public async Task<Address> Save(Address address)
  {
    _context.Addresses.Update(address);
    await _context.SaveChangesAsync();
    return address;
  }
}