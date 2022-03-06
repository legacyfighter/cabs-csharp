using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Repository;

public class TransactionalAddressRepository : IAddressRepository
{
  private readonly IAddressRepository _inner;
  private readonly ITransactions _transactions;

  public TransactionalAddressRepository(IAddressRepository inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<Address> Save(Address address)
  {
    return await _inner.Save(address);
  }

  public async Task<Address> Find(long? id)
  {
    return await _inner.Find(id);
  }

  public async Task<int?> FindHashById(long? addressId)
  {
    await using var tx = await _transactions.BeginTransaction();
    var hash = await _inner.FindHashById(addressId);
    await tx.Commit();
    return hash;
  }

  public async Task<Address> GetByHash(int hash)
  {
    return await _inner.GetByHash(hash);
  }
}