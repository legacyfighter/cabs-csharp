using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Parties.Api;

namespace LegacyFighter.Cabs.Repair.Api;

public class TransactionalContractManager : IContractManager
{
  private readonly IContractManager _inner;
  private readonly ITransactions _transactions;

  public TransactionalContractManager(IContractManager inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task ExtendedWarrantyContractSigned(PartyId insurerId, PartyId vehicleId)
  {
    await using var transaction = await _transactions.BeginTransaction();
    await _inner.ExtendedWarrantyContractSigned(insurerId, vehicleId);
    await transaction.Commit();
  }

  public async Task ManufacturerWarrantyRegistered(PartyId distributorId, PartyId vehicleId)
  {
    await using var transaction = await _transactions.BeginTransaction();
    await _inner.ManufacturerWarrantyRegistered(distributorId, vehicleId);
    await transaction.Commit();
  }
}