using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Crm.Claims;

public class TransactionalClaimService : IClaimService
{
  private readonly IClaimService _inner;
  private readonly ITransactions _transactions;

  public TransactionalClaimService(IClaimService inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<Claim> Create(ClaimDto claimDto)
  {
    return await _inner.Create(claimDto);
  }

  public Task<Claim> Find(long? id)
  {
    return _inner.Find(id);
  }

  public async Task<Claim> Update(ClaimDto claimDto, Claim claim)
  {
    return await _inner.Update(claimDto, claim);
  }

  public async Task<Claim> SetStatus(Statuses newStatus, long? id)
  {
    await using var tx = await _transactions.BeginTransaction();
    var status = await _inner.SetStatus(newStatus, id);
    await tx.Commit();
    return status;
  }

  public async Task<Claim> TryToResolveAutomatically(long? id)
  {
    await using var tx = await _transactions.BeginTransaction();
    var claim = await _inner.TryToResolveAutomatically(id);
    await tx.Commit();
    return claim;
  }

  public async Task<int> GetNumberOfClaims(long? clientId)
  {
    return await _inner.GetNumberOfClaims(clientId);
  }
}