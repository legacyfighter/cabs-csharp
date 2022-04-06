using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Entity;

public class Client : BaseEntity
{

  public enum Types
  {
    Normal,
    Vip
  }

  public enum ClientTypes
  {
    Individual,
    Company
  }

  public enum PaymentTypes
  {
    PrePaid,
    PostPaid,
    MonthlyInvoice
  }

  public Client()
  {

  }

  public string Name { get; set; }
  public string LastName { get; set; }
  public ClientTypes? ClientType { get; set; }
  public Types? Type { get; set; }
  public PaymentTypes? DefaultPaymentType { get; set; }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as Client)?.Id;
  }

  public static bool operator ==(Client left, Client right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(Client left, Client right)
  {
    return !Equals(left, right);
  }
}