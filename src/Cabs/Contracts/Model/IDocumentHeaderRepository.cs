namespace LegacyFighter.Cabs.Contracts.Model;

public interface IDocumentHeaderRepository
{
  Task<DocumentHeader> Find(long? id);
  Task Save(DocumentHeader header);
}
