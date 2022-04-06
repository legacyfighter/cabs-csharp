using LegacyFighter.Cabs.Crm.Claims;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.CabsTests.Common;

public class ClaimFixture
{
  private readonly IClaimService _claimService;

  public ClaimFixture(IClaimService claimService)
  {
    _claimService = claimService;
  }

  public async Task<Claim> CreateClaim(Client client, Transit transit)
  {
    var claimDto = ClaimDto("Okradli mnie na hajs", "$$$", client.Id, transit.Id);
    claimDto.IsDraft = false;
    return await _claimService.Create(claimDto);
  }

  public async Task<Claim> CreateClaim(Client client, Transit transit, string reason) 
  {
    var claimDto = ClaimDto("Okradli mnie na hajs", reason, client.Id, transit.Id);
    claimDto.IsDraft = false;
    return await _claimService.Create(claimDto);
  }

  public async Task<Claim> CreateAndResolveClaim(Client client, Transit transit) 
  {
    var claim = await CreateClaim(client, transit);
    claim = await _claimService.TryToResolveAutomatically(claim.Id);
    return claim;
  }

  private ClaimDto ClaimDto(string desc, string reason, long? clientId, long? transitId) 
  {
    var claimDto = new ClaimDto
    {
      ClientId = clientId,
      TransitId = transitId,
      IncidentDescription = desc,
      Reason = reason
    };
    return claimDto;
  }
}