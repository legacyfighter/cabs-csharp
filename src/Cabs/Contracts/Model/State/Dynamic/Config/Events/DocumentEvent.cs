using LegacyFighter.Cabs.Contracts.Model.Content;
using MediatR;

namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Events;

public abstract class DocumentEvent : INotification
{
  public DocumentEvent(long? documentId, string currentSate, ContentId contentId, DocumentNumber number)
  {
    DocumentId = documentId;
    CurrentSate = currentSate;
    ContentId = contentId;
    Number = number;
  }

  public long? DocumentId { get; }
  public string CurrentSate { get; }
  public ContentId ContentId { get; }
  public DocumentNumber Number { get; }
}