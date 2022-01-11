using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Entity;

public class Driver : BaseEntity
{

  public Driver()
  {

  }

  public enum Types
  {
    Candidate,
    Regular
  }

  public enum Statuses
  {
    Active,
    Inactive

  }

  public decimal? CalculateEarningsForTransit(Transit transit)
  {
    return null;
  }

  public virtual ISet<DriverAttribute> Attributes { get; set; } = new HashSet<DriverAttribute>();
  public Types? Type { get; set; }
  public Statuses Status { get; set; }
  public string FirstName { get; set; }
  public string LastName { get; set; }
  public string Photo { get; set; }
  public DriverLicense DriverLicense { get; set; }
  public virtual DriverFee Fee { get; set; }
  public bool Occupied { get; set; }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as Driver)?.Id;
  }

  public static bool operator ==(Driver left, Driver right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(Driver left, Driver right)
  {
    return !Equals(left, right);
  }

  public virtual ISet<Transit> Transits { get; private set; } = new HashSet<Transit>();
  public virtual ISet<Transit> RejectingTransits { get; private set; } = new HashSet<Transit>();
  public virtual ISet<Transit> ProposingTransits { get; private set; } = new HashSet<Transit>();
}