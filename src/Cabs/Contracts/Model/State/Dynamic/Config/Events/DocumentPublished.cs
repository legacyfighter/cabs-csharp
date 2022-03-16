using LegacyFighter.Cabs.Contracts.Model.Content;

namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Events;

public class DocumentPublished : DocumentEvent
{
  public DocumentPublished(long? documentId, string currentSate, ContentId contentId, DocumentNumber number)
    : base(documentId, currentSate, contentId, number)
  {
  }
}