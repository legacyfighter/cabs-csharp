using LegacyFighter.Cabs.Contracts.Model.Content;

namespace LegacyFighter.Cabs.Contracts.Application.Editor;

public class DocumentDto
{
  public DocumentDto(Guid? contentId, string physicalContent, ContentVersion contentVersion)
  {
    ContentId = contentId;
    PhysicalContent = physicalContent;
    DocumentVersion = contentVersion;
  }

  public Guid? ContentId { get; }
  public ContentVersion DocumentVersion { get; }
  public string PhysicalContent { get; }
}