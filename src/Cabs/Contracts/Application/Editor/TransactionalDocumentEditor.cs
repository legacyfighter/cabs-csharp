using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Contracts.Application.Editor;

internal class TransactionalDocumentEditor : IDocumentEditor
{
  private readonly DocumentEditor _inner;
  private readonly ITransactions _transactions;

  public TransactionalDocumentEditor(DocumentEditor inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public async Task<CommitResult> Commit(DocumentDto document)
  {
    await using var tx = await _transactions.BeginTransaction();
    var commitResult = await _inner.Commit(document);
    await tx.Commit();
    return commitResult;
  }

  public async Task<DocumentDto> Get(Guid contentId)
  {
    await using var tx = await _transactions.BeginTransaction();
    var documentDto = await _inner.Get(contentId);
    await tx.Commit();
    return documentDto;
  }
}