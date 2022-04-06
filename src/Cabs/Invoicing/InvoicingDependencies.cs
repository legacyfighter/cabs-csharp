namespace LegacyFighter.Cabs.Invoicing;

public static class InvoicingDependencies
{
  public static void AddTo(WebApplicationBuilder builder)
  {
    builder.Services.AddTransient<InvoiceGenerator>();
    builder.Services.AddTransient<IInvoiceRepository, EfCoreInvoiceRepository>();
  }
}