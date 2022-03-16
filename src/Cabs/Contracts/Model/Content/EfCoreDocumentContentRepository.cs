using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.Contracts.Model.Content;

public interface IDocumentContentRepository
{
  Task<DocumentContent> Find(Guid contentId);
  Task Save(DocumentContent content);
}

internal class EfCoreDocumentContentRepository : IDocumentContentRepository
{
  private readonly SqLiteDbContext _dbContext;

  public EfCoreDocumentContentRepository(SqLiteDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<DocumentContent> Find(Guid contentId)
  {
    return await _dbContext.DocumentContents.FindAsync(contentId);
  }

  public async Task Save(DocumentContent content)
  {
    _dbContext.DocumentContents.Update(content);
    await _dbContext.SaveChangesAsync();
  }
}