using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Common;

public class Transactions : ITransactions
{
  private readonly DbContext _context;

  public Transactions(DbContext context)
  {
    _context = context;
  }

  public async Task<ITransaction> BeginTransaction()
  {
    if (_context.Database.CurrentTransaction == null)
    {
      return new Transaction(
        await _context.Database.BeginTransactionAsync(), _context);
    }
    else
    {
      return new NullTransaction();
    }
  }
}