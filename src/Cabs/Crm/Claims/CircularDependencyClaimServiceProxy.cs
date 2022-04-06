namespace LegacyFighter.Cabs.Crm.Claims;

internal class CircularDependencyClaimServiceProxy : IClaimService
{
  private readonly Lazy<IClaimService> _lazyEvaluatedClaimServiceCreation;

  public CircularDependencyClaimServiceProxy(Func<IClaimService> claimServiceFactory)
  {
    _lazyEvaluatedClaimServiceCreation = new Lazy<IClaimService>(claimServiceFactory);
  }

  public async Task<Claim> Create(ClaimDto claimDto)
  {
    return await _lazyEvaluatedClaimServiceCreation.Value.Create(claimDto);
  }

  public async Task<Claim> Find(long? id)
  {
    return await _lazyEvaluatedClaimServiceCreation.Value.Find(id);
  }

  public async Task<Claim> Update(ClaimDto claimDto, Claim claim)
  {
    return await _lazyEvaluatedClaimServiceCreation.Value.Update(claimDto, claim);
  }

  public async Task<Claim> SetStatus(Statuses newStatus, long? id)
  {
    return await _lazyEvaluatedClaimServiceCreation.Value.SetStatus(newStatus, id);
  }

  public async Task<Claim> TryToResolveAutomatically(long? id)
  {
    return await _lazyEvaluatedClaimServiceCreation.Value.TryToResolveAutomatically(id);
  }

  public Task<int> GetNumberOfClaims(long? clientId)
  {
    return _lazyEvaluatedClaimServiceCreation.Value.GetNumberOfClaims(clientId);
  }
}