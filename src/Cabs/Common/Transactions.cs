using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.Common;

public class Transactions : ITransactions
{
  private readonly SqLiteDbContext _context;

  public Transactions(SqLiteDbContext context)
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