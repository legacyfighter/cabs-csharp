
namespace LegacyFighter.Cabs.Contracts.Model.Content;

public class DocumentContent
{
  public Guid Id { get; private set; }
  private Guid? PreviousId { get; set; }
  public string PhysicalContent { get; private set; } //some kind of reference to file, version control. In sour sample i will be a blob stored in DB:)
  public ContentVersion DocumentVersion { get; } //just a human readable descriptor

  protected DocumentContent()
  {
  }

  public DocumentContent(Guid? previousId, ContentVersion contentVersion, string physicalContent)
  {
    PreviousId = previousId;
    DocumentVersion = contentVersion;
    PhysicalContent = physicalContent;
  }

  public DocumentContent(ContentVersion version, string physicalContent)
    : this(null, version, physicalContent)
  {
  }
}