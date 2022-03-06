using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Service;
using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.Controllers;

[ApiController]
[Route("[controller]")]
public class TransitAnalyzerController
{
  private readonly ITransitAnalyzer _transitAnalyzer;

  public TransitAnalyzerController(ITransitAnalyzer transitAnalyzer)
  {
    _transitAnalyzer = transitAnalyzer;
  }

  [HttpGet("/transitAnalyze/{clientId}/{addressId}")]
  public async Task<AnalyzedAddressesDto> Analyze( long? clientId, long? addressId)
  {
    var addresses = await _transitAnalyzer.Analyze(clientId, addressId);
    var addressDtOs = addresses
      .Select(a=> new AddressDto(a))
      .ToList();

    return new AnalyzedAddressesDto(addressDtOs);
  }
}