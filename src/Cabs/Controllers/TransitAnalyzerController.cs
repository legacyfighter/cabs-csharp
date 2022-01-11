using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Service;
using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.Controllers;

[ApiController]
[Route("[controller]")]
public class TransitAnalyzerController
{
  internal ITransitAnalyzer TransitAnalyzer;

  public TransitAnalyzerController(ITransitAnalyzer transitAnalyzer)
  {
    TransitAnalyzer = transitAnalyzer;
  }

  [HttpGet("/transitAnalyze/{clientId}/{addressId}")]
  public async Task<AnalyzedAddressesDto> Analyze( long? clientId, long? addressId)
  {
    var addresses = await TransitAnalyzer.Analyze(clientId, addressId);
    var addressDtOs = addresses
      .Select(a=> new AddressDto(a))
      .ToList();

    return new AnalyzedAddressesDto(addressDtOs);
  }
}