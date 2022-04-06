using System;
using LegacyFighter.Cabs.Geolocation;
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

  public async Task<AddressDto> AnAddress(
    IGeocodingService geocodingService,
    string country,
    string city,
    string street,
    int buildingNumber)
  {
    var addressDto = new AddressDto(country, city, street, buildingNumber);
    var random = new Random();
    geocodingService.GeocodeAddress(
        Arg.Is<Address>(a => new AddressMatcher(addressDto).Matches(a)))
      .Returns(new double[] { random.Next(), random.Next() });
    return addressDto;
  }
}