using System.Data.Common;
using LegacyFighter.Cabs.Common;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace LegacyFighter.Cabs.Repository;

public class EventFlushInterceptor : DbTransactionInterceptor
{
  private readonly EventsPublisher _eventsPublisher;

  public EventFlushInterceptor(EventsPublisher eventsPublisher)
  {
    _eventsPublisher = eventsPublisher;
  }

  public override async Task TransactionCommittedAsync(DbTransaction transaction, TransactionEndEventData eventData,
    CancellationToken cancellationToken = new())
  {
    await _eventsPublisher.Flush();
  }
}