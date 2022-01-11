using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Entity;

public class Invoice : BaseEntity
{
  protected Invoice()
  {

  }

  public decimal Amount { get; private set; }
  public string SubjectName { get; private set; }

  public Invoice(decimal amount, string subjectName)
  {
    Amount = amount;
    SubjectName = subjectName;
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as Invoice)?.Id;
  }

  public static bool operator ==(Invoice left, Invoice right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(Invoice left, Invoice right)
  {
    return !Equals(left, right);
  }
}