namespace LegacyFighter.Cabs.Assignment;

public class InvolvedDriversSummary
{
  public ISet<long?> ProposedDrivers { get; set; } = new HashSet<long?>();
  public ISet<long?> DriverRejections { get; set; } = new HashSet<long?>();
  public long? AssignedDriver { get; set; }
  public AssignmentStatuses Status { get; set; }

  public InvolvedDriversSummary()
  {
  }

  public InvolvedDriversSummary(
    ISet<long?> proposedDrivers,
    ISet<long?> driverRejections,
    long? assignedDriverId,
    AssignmentStatuses status)
  {
    ProposedDrivers = proposedDrivers;
    DriverRejections = driverRejections;
    Status = status;
  }

  public static InvolvedDriversSummary NoneFound()
  {
    return new InvolvedDriversSummary(
      new HashSet<long?>(),
      new HashSet<long?>(),
      null, 
      AssignmentStatuses.DriverAssignmentFailed);
  }
}
