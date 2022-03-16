using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Contracts.Model.Content;

namespace LegacyFighter.Cabs.Contracts.Model;

public class DocumentHeader : BaseEntity
{
  public DocumentNumber DocumentNumber { get; }
  public long? AuthorId { get; set; }

  public long? VerifierId { private get; set; }
  public long? Verifier => VerifierId;
  public string StateDescriptor { get; set; }

  public ContentId ContentId { get; private set; }

  protected DocumentHeader()
  {
  }

  public DocumentHeader(long? authorId, DocumentNumber number)
  {
    AuthorId = authorId;
    DocumentNumber = number;
  }

  public void ChangeCurrentContent(ContentId contentId)
  {
    ContentId = contentId;
  }

  public bool NotEmpty()
  {
    return ContentId != null;
  }
}