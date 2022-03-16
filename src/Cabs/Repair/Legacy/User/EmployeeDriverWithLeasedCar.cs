using LegacyFighter.Cabs.Repair.Legacy.Job;

namespace LegacyFighter.Cabs.Repair.Legacy.User;

public class EmployeeDriverWithLeasedCar : EmployeeDriver
{
  protected override JobResult Handle(RepairJob job)
  {
    return new JobResult(JobResult.Decisions.Redirection).AddParam("shouldHandleBy", LasingCompanyId);
  }

  public long? LasingCompanyId { get; set; }
}