using LegacyFighter.Cabs.Contracts.Model.Content;

namespace LegacyFighter.Cabs.Contracts.Application.Editor;

public class DocumentEditor : IDocumentEditor
{
  private readonly IDocumentContentRepository _documentContentRepository;

  public DocumentEditor(IDocumentContentRepository documentContentRepository)
  {
    _documentContentRepository = documentContentRepository;
  }

  public async Task<CommitResult> Commit(DocumentDto document)
  {
    var previousId = document.ContentId;
    var content = new DocumentContent(previousId, document.DocumentVersion, document.PhysicalContent);
    await _documentContentRepository.Save(content);
    return new CommitResult(content.Id, CommitResult.Results.Success);
  }

  public async Task<DocumentDto> Get(Guid contentId)
  {
    var content = await _documentContentRepository.Find(contentId);
    return new DocumentDto(contentId, content.PhysicalContent, content.DocumentVersion);
  }
}