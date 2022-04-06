using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Invoicing;

public class Invoice : BaseEntity
{
  private decimal Amount { get; set; }
  private string SubjectName { get; set; }

  public Invoice(decimal amount, string subjectName)
  {
    Amount = amount;
    SubjectName = subjectName;
  }

  protected Invoice()
  {
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