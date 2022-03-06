using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Repository;

public interface IAddressRepository
{
  Task<Address> Save(Address address);
  Task<Address> Find(long? id);
  Task<int?> FindHashById(long? addressId);
  Task<Address> GetByHash(int hash);
}

public class AddressRepository : IAddressRepository
{
  private readonly IAddressRepositoryInterface _addressRepositoryInterface;

  // FIX ME: To replace with getOrCreate method instead of that?
  // Actual workaround for address uniqueness problem: assign result from repo.save to variable for later usage
  public AddressRepository(IAddressRepositoryInterface addressRepositoryInterface)
  {
    _addressRepositoryInterface = addressRepositoryInterface;
  }

  public async Task<Address> Save(Address address)
  {
    address.UpdateHash();

    if (address.Id == null)
    {
      var existingAddress = await _addressRepositoryInterface.FindByHash(address.Hash);

      if (existingAddress != null)
      {
        return existingAddress;
      }
    }

    return await _addressRepositoryInterface.Save(address);
  }

  public async Task<Address> Find(long? id)
  {
    return await _addressRepositoryInterface.Find(id);
  }

  public async Task<int?> FindHashById(long? addressId) 
  {
    return await _addressRepositoryInterface.FindHashById(addressId);
  }

  public async Task<Address> GetByHash(int hash) 
  {
    return await _addressRepositoryInterface.FindByHash(hash);
  }
}