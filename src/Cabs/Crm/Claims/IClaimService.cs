namespace LegacyFighter.Cabs.Crm.Claims;

public interface IClaimService
{
  Task<Claim> Create(ClaimDto claimDto);
  Task<Claim> Find(long? id);
  Task<Claim> Update(ClaimDto claimDto, Claim claim);
  Task<Claim> SetStatus(Statuses newStatus, long? id);
  Task<Claim> TryToResolveAutomatically(long? id);
  Task<int> GetNumberOfClaims(long? clientId);
}