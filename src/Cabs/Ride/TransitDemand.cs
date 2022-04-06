using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Ride;

public class TransitDemand : BaseEntity
{
  private Guid _transitRequestGuid;

  public enum Statuses
  {
    Cancelled,
    WaitingForDriverAssignment,
    TransitToPassenger,
  }

  public Statuses Status { get; private set; }
  private int PickupAddressChangeCounter { get; set; } = 0;

  protected TransitDemand()
  {

  }

  public TransitDemand(Guid transitRequestGuid)
  {
    _transitRequestGuid = transitRequestGuid;
    Status = Statuses.WaitingForDriverAssignment;
  }

  public void ChangePickup(double distanceFromPreviousPickup)
  {
    if (distanceFromPreviousPickup > 0.25)
    {
      throw new InvalidOperationException($"Address 'from' cannot be changed, id = {Id}");
    }
    else if (Status != Statuses.WaitingForDriverAssignment)
    {
      throw new InvalidOperationException($"Address 'from' cannot be changed, id = {Id}");
    }
    else if (PickupAddressChangeCounter > 2)
    {
      throw new InvalidOperationException($"Address 'from' cannot be changed, id = {Id}");
    }

    PickupAddressChangeCounter += 1;
  }

  public void Accept()
  {
    Status = Statuses.TransitToPassenger;
  }

  public void Cancel()
  {
    if (Status != Statuses.WaitingForDriverAssignment)
    {
      throw new InvalidOperationException($"Demand cannot be cancelled, id = {Id}");
    }
    Status = Statuses.Cancelled;
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as TransitDemand)?.Id;
  }

  public static bool operator ==(TransitDemand left, TransitDemand right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(TransitDemand left, TransitDemand right)
  {
    return !Equals(left, right);
  }
}
