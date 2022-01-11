using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity;

public class AwardedMiles : BaseEntity
{
  // Aggregate
  // 1. mile celowo są osobno, aby się mogło rozjechać na ich wydawaniu -> docelowo: kolekcja VOs w agregacie

  // VO
  // 1. miles + expirationDate -> VO przykrywające logikę walidacji, czy nie przekroczono daty ważności punktów
  // 2. wydzielenie interfejsu Miles -> różne VO z różną logiką, np. ExpirableMiles, NonExpirableMiles, LinearExpirableMiles

  public AwardedMiles()
  {
  }

  public virtual Client Client { get; set; }
  public int Miles { get; set; }
  public Instant Date { get; set; } = SystemClock.Instance.GetCurrentInstant();
  public Instant? ExpirationDate { get; set; }
  public bool IsSpecial { get; set; }
  public virtual Transit Transit { get; set; }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as AwardedMiles)?.Id;
  }

  public static bool operator ==(AwardedMiles left, AwardedMiles right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(AwardedMiles left, AwardedMiles right)
  {
    return !Equals(left, right);
  }
}