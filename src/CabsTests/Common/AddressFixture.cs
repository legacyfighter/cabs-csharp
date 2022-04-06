using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.CabsTests.Common;

public class AddressFixture
{
  private readonly IAddressRepository _addressRepository;

  public AddressFixture(IAddressRepository addressRepository)
  {
    _addressRepository = addressRepository;
  }

  public async Task<Address> AnAddress()
  {
    return await _addressRepository.Save(new Address("Polska", "Warszawa", "M³ynarska", 20));
  }
}