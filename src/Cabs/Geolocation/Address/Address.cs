using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Geolocation.Address;

public class Address : BaseEntity
{
  public Address()
  {

  }

  public Address(string country, string city, string street, int buildingNumber)
  {
    Country = country;
    City = city;
    Street = street;
    BuildingNumber = buildingNumber;
  }

  public Address(string country, string district, string city, string street, int buildingNumber) 
  {
    Country = country;
    District = district;
    City = city;
    Street = street;
    BuildingNumber = buildingNumber;
  }

  public string Country { set; get; }
  public string District { get; set; }
  public string City { get; set; }
  public string Street { get; set; }
  public int? BuildingNumber { get; set; }
  public int? AdditionalNumber { get; set; }
  public string PostalCode { get; set; }
  public string Name { get; set; }
  public int Hash { get; private set; }

  public void UpdateHash()
  {
    Hash = HashCode.Combine(Country, District, City, Street, BuildingNumber, AdditionalNumber, PostalCode, Name);
  }

  public override string ToString()
  {
    return "Address{" +
           "id='" + Id + '\'' +
           ", country='" + Country + '\'' +
           ", district='" + District + '\'' +
           ", city='" + City + '\'' +
           ", street='" + Street + '\'' +
           ", buildingNumber=" + BuildingNumber +
           ", additionalNumber=" + AdditionalNumber +
           ", postalCode='" + PostalCode + '\'' +
           ", name='" + Name + '\'' +
           '}';
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as Address)?.Id;
  }

  public static bool operator ==(Address left, Address right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(Address left, Address right)
  {
    return !Equals(left, right);
  }
}