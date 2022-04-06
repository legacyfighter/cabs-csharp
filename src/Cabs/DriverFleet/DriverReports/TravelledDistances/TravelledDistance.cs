using LegacyFighter.Cabs.DistanceValue;
using NodaTime;
using NodaTime.TimeZones;

namespace LegacyFighter.Cabs.DriverFleet.DriverReports.TravelledDistances;

public class TravelledDistance
{
  private Guid IntervalId { get; set; } = Guid.NewGuid();

  private long _driverId;
  protected virtual TimeSlot TimeSlot { get; set; }
  protected virtual Distance Distance { get; set; }
  public double LastLongitude { get; protected set; }
  public double LastLatitude { get; protected set; }

  protected TravelledDistance()
  {
  }

  public TravelledDistance(long driverId, TimeSlot timeSlot, double lastLatitude, double lastLongitude)
  {
    _driverId = driverId;
    TimeSlot = timeSlot;
    LastLatitude = lastLatitude;
    LastLongitude = lastLongitude;
    Distance = Distance.Zero;
  }

  internal bool Contains(Instant timestamp)
  {
    return TimeSlot.Contains(timestamp);
  }

  internal void AddDistance(Distance travelled, double latitude, double longitude)
  {
    Distance = Distance.Add(travelled);
    LastLatitude = latitude;
    LastLongitude = longitude;
  }

  internal bool EndsAt(Instant instant)
  {
    return TimeSlot.EndsAt(instant);
  }

  internal bool IsBefore(Instant now)
  {
    return TimeSlot.IsBefore(now);
  }
}

public class TimeSlot : IEquatable<TimeSlot>
{
  private static readonly int FiveMinutes = 300;

  public Instant Beginning { get; }
  public Instant End { get; }

  protected TimeSlot()
  {
  }

  private TimeSlot(Instant beginning, Instant end)
  {
    Beginning = beginning;
    End = end;
  }

  public static TimeSlot Of(Instant beginning, Instant end)
  {
    if (end <= beginning)
    {
      throw new ArgumentException($"From {beginning} is after to {end}");
    }

    return new TimeSlot(beginning, end);
  }

  public static TimeSlot ThatContains(Instant seed)
  {
    var startOfDay = seed.InZone(BclDateTimeZone.ForSystemDefault()).Date
      .AtStartOfDayInZone(BclDateTimeZone.ForSystemDefault()).LocalDateTime;
    var seedDateTime = seed.InZone(BclDateTimeZone.ForSystemDefault()).LocalDateTime;
    var secondsFromStartOfDay = (seedDateTime - startOfDay).ToDuration().TotalSeconds;
    var intervals = (long)Math.Floor((secondsFromStartOfDay / (double)FiveMinutes));
    var from = startOfDay.PlusSeconds(intervals * FiveMinutes).InZoneStrictly(BclDateTimeZone.ForSystemDefault())
      .ToInstant();
    return new TimeSlot(from, from.Plus(Duration.FromSeconds(FiveMinutes)));
  }

  public bool Contains(Instant timestamp)
  {
    return timestamp < End && !(Beginning > timestamp);
  }

  public bool EndsAt(Instant timestamp)
  {
    return End == timestamp;
  }

  public bool IsBefore(Instant timestamp)
  {
    return End < timestamp;
  }

  public TimeSlot Prev()
  {
    return new TimeSlot(
      Beginning.Minus(Duration.FromSeconds(FiveMinutes)),
      End.Minus(Duration.FromSeconds(FiveMinutes)));
  }

  public bool Equals(TimeSlot other)
  {
    if (ReferenceEquals(null, other)) return false;
    if (ReferenceEquals(this, other)) return true;
    return Beginning.Equals(other.Beginning) && End.Equals(other.End);
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != GetType()) return false;
    return Equals((TimeSlot)obj);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine(Beginning, End);
  }

  public static bool operator ==(TimeSlot left, TimeSlot right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(TimeSlot left, TimeSlot right)
  {
    return !Equals(left, right);
  }

  public override string ToString()
  {
    return "Slot{" +
           "beginning=" + Beginning +
           ", end=" + End +
           '}';
  }
}