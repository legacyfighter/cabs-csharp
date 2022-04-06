using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.Cabs.Crm.TransitAnalyzer;

public class AnalyzedAddressesDto
{
  public AnalyzedAddressesDto()
  {
  }

  public AnalyzedAddressesDto(List<AddressDto> addresses)
  {
    Addresses = addresses;
  }

  public List<AddressDto> Addresses { get; set; }
}