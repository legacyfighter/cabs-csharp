using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Assignment;

public class DriverAssignment : BaseEntity
{
  private readonly Instant _publishedAt;
  private string _driversRejections;
  private string _proposedDrivers;

  internal Guid RequestId { get; }
  internal AssignmentStatuses Status { get; private set; } = AssignmentStatuses.WaitingForDriverAssignment;
  internal long? AssignedDriver { get; private set; }
  internal int? AwaitingDriversResponses { get; private set; } = 0;

  public DriverAssignment()
  {
  }

  public DriverAssignment(Guid requestId, Instant publishedAt)
  {
    RequestId = requestId;
    _publishedAt = publishedAt;
  }

  internal void Cancel()
  {
    if (!new HashSet<AssignmentStatuses> { AssignmentStatuses.WaitingForDriverAssignment, AssignmentStatuses.OnTheWay }
          .Contains(Status))
    {
      throw new InvalidOperationException("Transit cannot be cancelled, id = " + Id);
    }

    Status = AssignmentStatuses.Cancelled;
    AssignedDriver = null;
    AwaitingDriversResponses = 0;
  }

  internal bool CanProposeTo(long? driverId)
  {
    return !DriverRejections
      .Contains(driverId);
  }

  internal void ProposeTo(long? driverId)
  {
    if (CanProposeTo(driverId))
    {
      AddDriverToProposed(driverId);
      AwaitingDriversResponses++;
    }
  }

  private void AddDriverToProposed(long? driverId)
  {
    var proposedDriversSet = ProposedDrivers;
    proposedDriversSet.Add(driverId);
    _proposedDrivers = JsonToCollectionMapper.Serialize(proposedDriversSet);
  }

  internal void FailDriverAssignment()
  {
    Status = AssignmentStatuses.DriverAssignmentFailed;
    AssignedDriver = null;
    AwaitingDriversResponses = 0;
  }

  internal bool ShouldNotWaitForDriverAnyMore(Instant date)
  {
    return Status == AssignmentStatuses.Cancelled ||
           _publishedAt.Plus(Duration.FromSeconds(300)) < date;
  }

  internal void AcceptBy(long? driverId)
  {
    if (AssignedDriver != null)
    {
      throw new InvalidOperationException($"Transit already accepted, id = {Id}");
    }
    else
    {
      if (!ProposedDrivers.Contains(driverId))
      {
        throw new InvalidOperationException($"Driver out of possible drivers, id = {Id}");
      }
      else
      {
        if (DriverRejections.Contains(driverId))
        {
          throw new InvalidOperationException($"Driver out of possible drivers, id = {Id}");
        }
      }

      AssignedDriver = driverId;
      AwaitingDriversResponses = 0;
      Status = AssignmentStatuses.OnTheWay;
    }
  }

  internal void RejectBy(long? driverId)
  {
    AddToDriverRejections(driverId);
    AwaitingDriversResponses--;
  }

  private void AddToDriverRejections(long? driverId)
  {
    var driverRejectionSet = DriverRejections;
    driverRejectionSet.Add(driverId);
    _driversRejections = JsonToCollectionMapper.Serialize(driverRejectionSet);
  }

  internal ISet<long?> DriverRejections
    => JsonToCollectionMapper.Deserialize(_driversRejections);

  internal ISet<long?> ProposedDrivers
    => JsonToCollectionMapper.Deserialize(_proposedDrivers);

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as DriverAssignment)?.Id;
  }

  public static bool operator ==(DriverAssignment left, DriverAssignment right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(DriverAssignment left, DriverAssignment right)
  {
    return !Equals(left, right);
  }
}