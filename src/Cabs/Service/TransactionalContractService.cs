using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public class TransactionalContractService : IContractService
{
  private readonly IContractService _inner;
  private readonly ITransactions _transactions;

  public TransactionalContractService(IContractService inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<Contract> CreateContract(ContractDto contractDto)
  {
    await using var tx = await _transactions.BeginTransaction();
    var contract = await _inner.CreateContract(contractDto);
    await tx.Commit();
    return contract;
  }

  public async Task AcceptContract(long? id)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.AcceptContract(id);
    await tx.Commit();
  }

  public async Task RejectContract(long? id)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.RejectContract(id);
    await tx.Commit();
  }

  public async Task RejectAttachment(long? attachmentId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.RejectAttachment(attachmentId);
    await tx.Commit();
  }

  public async Task AcceptAttachment(long? attachmentId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.AcceptAttachment(attachmentId);
    await tx.Commit();
  }

  public async Task<Contract> Find(long? id)
  {
    await using var tx = await _transactions.BeginTransaction();
    var contract = await _inner.Find(id);
    await tx.Commit();
    return contract;
  }

  public async Task<ContractDto> FindDto(long? id)
  {
    await using var tx = await _transactions.BeginTransaction();
    var contractDto = await _inner.FindDto(id);
    await tx.Commit();
    return contractDto;
  }

  public async Task<ContractAttachmentDto> ProposeAttachment(long? contractId, ContractAttachmentDto contractAttachmentDto)
  {
    await using var tx = await _transactions.BeginTransaction();
    var attachment = await _inner.ProposeAttachment(contractId, contractAttachmentDto);
    await tx.Commit();
    return attachment;
  }

  public async Task RemoveAttachment(long? contractId, long? attachmentId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.RemoveAttachment(contractId, attachmentId);
    await tx.Commit();
  }
}