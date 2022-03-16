namespace LegacyFighter.Cabs.Contracts.Application.Editor;

public interface IDocumentEditor
{
  Task<CommitResult> Commit(DocumentDto document);
  Task<DocumentDto> Get(Guid contentId);
}