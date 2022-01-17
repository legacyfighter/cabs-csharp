using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public class TransactionalAwardsService : IAwardsService
{
  private readonly IAwardsService _inner;
  private readonly ITransactions _transactions;

  public TransactionalAwardsService(IAwardsService inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<AwardsAccountDto> FindBy(long? clientId)
  {
    return await _inner.FindBy(clientId);
  }

  public async Task RegisterToProgram(long? clientId)
  {
    await _inner.RegisterToProgram(clientId);
  }

  public async Task ActivateAccount(long? clientId)
  {
    await _inner.ActivateAccount(clientId);
  }

  public async Task DeactivateAccount(long? clientId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.DeactivateAccount(clientId);
    await tx.Commit();
  }

  public async Task<AwardedMiles> RegisterMiles(long? clientId, long? transitId)
  {
    return await _inner.RegisterMiles(clientId, transitId);
  }

  public async Task<AwardedMiles> RegisterNonExpiringMiles(long? clientId, int miles)
  {
    return await _inner.RegisterNonExpiringMiles(clientId, miles);
  }

  public async Task RemoveMiles(long? clientId, int miles)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.RemoveMiles(clientId, miles);
    await tx.Commit();
  }

  public async Task<int> CalculateBalance(long? clientId)
  {
    return await _inner.CalculateBalance(clientId);
  }

  public async Task TransferMiles(long? fromClientId, long? toClientId, int miles)
  {
    await _inner.TransferMiles(fromClientId, toClientId, miles);
  }
}