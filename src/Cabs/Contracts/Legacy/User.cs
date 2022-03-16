namespace LegacyFighter.Cabs.Contracts.Legacy;

using Common;

public class User : BaseEntity
{
  protected virtual ISet<Document> AssignedDocuments { get; set; }
  protected virtual ISet<Document> CreatedDocuments { get; set; }
  protected virtual ISet<Document> VerifiedDocuments { get; set; }
}