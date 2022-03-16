using LegacyFighter.Cabs.Contracts.Model;
using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.Contracts.Infra;

public class EfCoreDocumentHeaderRepository : IDocumentHeaderRepository
{
  private readonly SqLiteDbContext _dbContext;

  public EfCoreDocumentHeaderRepository(SqLiteDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<DocumentHeader> Find(long? id)
  {
    return await _dbContext.DocumentHeaders.FindAsync(id);
  }

  public async Task Save(DocumentHeader header)
  {
    _dbContext.DocumentHeaders.Update(header);
    await _dbContext.SaveChangesAsync();
  }
}