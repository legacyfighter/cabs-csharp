namespace LegacyFighter.Cabs.Invoicing;

public class InvoiceGenerator
{
  private readonly IInvoiceRepository _invoiceRepository;

  public InvoiceGenerator(IInvoiceRepository invoiceRepository)
  {
    _invoiceRepository = invoiceRepository;
  }

  public async Task<Invoice> Generate(int? amount, string subjectName)
  {
    return await _invoiceRepository.Save(new Invoice(new decimal(amount.Value), subjectName));
  }
}