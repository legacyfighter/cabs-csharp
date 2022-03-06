using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity.Events;

public class TransitCompleted : IEvent
{
  public long? ClientId { get; }
  public long? TransitId { get; }
  public int? AddressFromHash { get; }
  public int? AddressToHash { get; }
  public Instant Started { get; }
  public Instant CompleteAt { get; }
  public Instant EventTimestamp { get; }

  public TransitCompleted(
    long? clientId,
    long? transitId,
    int? addressFromHash,
    int? addressToHash,
    Instant started,
    Instant completeAt,
    Instant eventTimestamp)
  {
    ClientId = clientId;
    TransitId = transitId;
    AddressFromHash = addressFromHash;
    AddressToHash = addressToHash;
    Started = started;
    CompleteAt = completeAt;
    EventTimestamp = eventTimestamp;
  }
}