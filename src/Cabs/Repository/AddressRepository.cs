using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Repository;

public class AddressRepository
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
}