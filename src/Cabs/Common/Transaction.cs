using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace LegacyFighter.Cabs.Common;

public interface ITransactions
{
  Task<ITransaction> BeginTransaction();
}

public class Transaction : ITransaction
{
  private readonly IDbContextTransaction _transaction;
  private readonly DbContext _dbContext;
  private bool _committed;

  public Transaction(IDbContextTransaction transaction, DbContext dbContext)
  {
    _transaction = transaction;
    _dbContext = dbContext;
  }

  public async Task Commit()
  {
    await _dbContext.SaveChangesAsync();
    await _transaction.CommitAsync();
    _committed = true;
  }

  public async ValueTask DisposeAsync()
  {
    if (!_committed)
    {
      await _transaction.RollbackAsync();
    }

    await _transaction.DisposeAsync();
  }
}