using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Contracts.Model;

namespace LegacyFighter.Cabs.Contracts.Application.Acme.Dynamic;

class TransactionalDocumentResourceManager : IDocumentResourceManager
{
  private readonly IDocumentResourceManager _inner;
  private readonly ITransactions _transactions;

  public TransactionalDocumentResourceManager(
    IDocumentResourceManager inner,
    ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task ChangeContent()
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.ChangeContent();
    await tx.Commit();
  }

  public async Task<DocumentOperationResult> ChangeContent(long? headerId, ContentId contentVersion)
  {
    await using var tx = await _transactions.BeginTransaction();
    var result = await _inner.ChangeContent(headerId, contentVersion);
    await tx.Commit();
    return result;
  }

  public async Task<DocumentOperationResult> CreateDocument(long? authorId)
  {
    await using var tx = await _transactions.BeginTransaction();
    var result = await _inner.CreateDocument(authorId);
    await tx.Commit();
    return result;
  }

  public async Task<DocumentOperationResult> ChangeState(long? documentId, string desiredState, Dictionary<string, object> @params)
  {
    await using var tx = await _transactions.BeginTransaction();
    var result = await _inner.ChangeState(documentId, desiredState, @params);
    await tx.Commit();
    return result;
  }
}