namespace LegacyFighter.Cabs.Dto;

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