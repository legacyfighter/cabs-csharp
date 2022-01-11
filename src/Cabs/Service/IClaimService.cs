using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public interface IClaimService
{
  Task<Claim> Create(ClaimDto claimDto);
  Task<Claim> Find(long? id);
  Task<Claim> Update(ClaimDto claimDto, Claim claim);
  Task<Claim> SetStatus(Claim.Statuses newStatus, long? id);
  Task<Claim> TryToResolveAutomatically(long? id);
}