using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore.Storage;

namespace LegacyFighter.Cabs.Common;

public interface ITransactions
{
  Task<ITransaction> BeginTransaction();
}

public class Transaction : ITransaction
{
  private readonly IDbContextTransaction _transaction;
  private readonly SqLiteDbContext _sqLiteDbContext;
  private bool _committed;

  public Transaction(IDbContextTransaction transaction, SqLiteDbContext sqLiteDbContext)
  {
    _transaction = transaction;
    _sqLiteDbContext = sqLiteDbContext;
  }

  public async Task Commit()
  {
    await _sqLiteDbContext.SaveChangesAsync();
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