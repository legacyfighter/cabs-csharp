using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.CabsTests.Common;

public class AddressMatcher
{
  private readonly string _country;
  private readonly string _city;
  private readonly string _street;
  private readonly int? _buildingNumber;

  public AddressMatcher(Address address) 
    : this(address.Country, address.City, address.Street, address.BuildingNumber)
  {
  }

  public AddressMatcher(AddressDto dto) : this(dto.ToAddressEntity())
  {
  }

  public AddressMatcher(string country, string city, string street, int? buildingNumber)
  {
    _country = country;
    _city = city;
    _street = street;
    _buildingNumber = buildingNumber;
  }

  public bool Matches(Address right)
  {
    if (right == null)
    {
      return false;
    }
    return _country == right.Country &&
           _city == right.City &&
           _street == right.Street &&
           _buildingNumber == right.BuildingNumber;
  }

}