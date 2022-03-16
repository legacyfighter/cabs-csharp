using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Contracts.Model;

namespace LegacyFighter.Cabs.Contracts.Application.Acme.Straightforward;

public class TransactionalAcmeContractProcessBasedOnStraightforwardDocumentModel : IAcmeContractProcessBasedOnStraightforwardDocumentModel
{
  private readonly IAcmeContractProcessBasedOnStraightforwardDocumentModel _inner;
  private readonly ITransactions _transactions;

  public TransactionalAcmeContractProcessBasedOnStraightforwardDocumentModel(
    IAcmeContractProcessBasedOnStraightforwardDocumentModel inner,
    ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<ContractResult> CreateContract(long? authorId)
  {
    await using var tx = await _transactions.BeginTransaction();
    var result = await _inner.CreateContract(authorId);
    await tx.Commit();
    return result;
  }

  public async Task<ContractResult> Verify(long? headerId, long? verifierId)
  {
    await using var tx = await _transactions.BeginTransaction();
    var result = await _inner.Verify(headerId, verifierId);
    await tx.Commit();
    return result;
  }

  public async Task<ContractResult> ChangeContent(long? headerId, ContentId contentVersion)
  {
    await using var tx = await _transactions.BeginTransaction();
    var result = await _inner.ChangeContent(headerId, contentVersion);
    await tx.Commit();
    return result;
  }
}