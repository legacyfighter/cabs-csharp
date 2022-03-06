using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Dto;

public class AddressDto
{

  public AddressDto()
  {

  }

  public AddressDto(Address a)
  {
    Country = a.Country;
    District = a.District;
    City = a.City;
    Street = a.Street;
    BuildingNumber = a.BuildingNumber;
    AdditionalNumber = a.AdditionalNumber;
    PostalCode = a.PostalCode;
    Name = a.Name;
    Hash = a.Hash;
  }

  public AddressDto(string country, string city, string street, int? buildingNumber)
  {
    Country = country;
    City = city;
    Street = street;
    BuildingNumber = buildingNumber;
  }

  public string Country { get; set; }

  public string District { get; set; }

  public string City { get; set; }

  public string Street { get; set; }

  public int? BuildingNumber { get; set; }

  public int? AdditionalNumber { get; set; }

  public string PostalCode { get; set; }

  public string Name { get; set; }

  public int Hash { get; set; }

  public Address ToAddressEntity()
  {
    var address = new Address
    {
      AdditionalNumber = AdditionalNumber,
      BuildingNumber = BuildingNumber,
      City = City,
      Name = Name,
      Street = Street,
      Country = Country,
      PostalCode = PostalCode,
      District = District
    };
    return address;
  }

  private string GetDebuggerDisplay()
  {
    return ToString();
  }
}