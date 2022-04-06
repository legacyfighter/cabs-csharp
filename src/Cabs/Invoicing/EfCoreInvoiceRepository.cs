using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.Invoicing;

public interface IInvoiceRepository
{
  Task<Invoice> Save(Invoice invoice);
}

internal class EfCoreInvoiceRepository : IInvoiceRepository
{
  private readonly SqLiteDbContext _context;

  public EfCoreInvoiceRepository(SqLiteDbContext context)
  {
    _context = context;
  }

  public async Task<Invoice> Save(Invoice invoice)
  {
    _context.Invoices.Update(invoice);
    await _context.SaveChangesAsync();
    return invoice;
  }
}