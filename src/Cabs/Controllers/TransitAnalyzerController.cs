using LegacyFighter.Cabs.Crm.TransitAnalyzer;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Repository;
using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.Controllers;

[ApiController]
[Route("[controller]")]
public class TransitAnalyzerController
{
  private readonly GraphTransitAnalyzer _graphTransitAnalyzer;
  private readonly IAddressRepository _addressRepository;

  public TransitAnalyzerController(
    GraphTransitAnalyzer graphTransitAnalyzer,
    IAddressRepository addressRepository)
  {
    _graphTransitAnalyzer = graphTransitAnalyzer;
    _addressRepository = addressRepository;
  }

  [HttpGet("/transitAnalyze/{clientId}/{addressId}")]
  public async Task<AnalyzedAddressesDto> Analyze(long? clientId, long? addressId)
  {
    
    var hashes = await _graphTransitAnalyzer.Analyze(clientId, await _addressRepository.FindHashById(addressId));
    var addressDtOs = (await Task.WhenAll(
      hashes.Select(async hash => await MapToAddressDto(hash)))).ToList();

    return new AnalyzedAddressesDto(addressDtOs);
  }

  private async Task<AddressDto> MapToAddressDto(long? hash) 
  {
    return new AddressDto(await _addressRepository.GetByHash((int)hash.Value));
  }
}