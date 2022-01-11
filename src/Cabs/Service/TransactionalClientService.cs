using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public class TransactionalClientService : IClientService
{
  private readonly IClientService _inner;
  private readonly ITransactions _transactions;

  public TransactionalClientService(IClientService inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<Client> RegisterClient(string name, string lastName, Client.Types? type, Client.PaymentTypes? paymentType)
  {
    await using var tx = await _transactions.BeginTransaction();
    var client = await _inner.RegisterClient(name, lastName, type, paymentType);
    await tx.Commit();
    return client;
  }

  public async Task ChangeDefaultPaymentType(long? clientId, Client.PaymentTypes? paymentType)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.ChangeDefaultPaymentType(clientId, paymentType);
    await tx.Commit();
  }

  public async Task UpgradeToVip(long? clientId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.UpgradeToVip(clientId);
    await tx.Commit();
  }

  public async Task DowngradeToRegular(long? clientId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.DowngradeToRegular(clientId);
    await tx.Commit();
  }

  public async Task<ClientDto> Load(long? id)
  {
    await using var tx = await _transactions.BeginTransaction();
    var clientDto = await _inner.Load(id);
    await tx.Commit();
    return clientDto;
  }
}