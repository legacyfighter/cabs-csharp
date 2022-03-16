using LegacyFighter.Cabs.Contracts.Model;

namespace LegacyFighter.Cabs.Contracts.Application.Acme.Dynamic;

public interface IDocumentResourceManager
{
  Task ChangeContent();
  Task<DocumentOperationResult> ChangeContent(long? headerId, ContentId contentVersion);
  Task<DocumentOperationResult> CreateDocument(long? authorId);
  Task<DocumentOperationResult> ChangeState(long? documentId, string desiredState, Dictionary<string, object> @params);
}