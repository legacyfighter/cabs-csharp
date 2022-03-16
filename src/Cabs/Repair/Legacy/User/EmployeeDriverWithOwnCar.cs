using LegacyFighter.Cabs.Repair.Legacy.Job;
using LegacyFighter.Cabs.Repair.Legacy.Parts;

namespace LegacyFighter.Cabs.Repair.Legacy.User;

public class EmployeeDriverWithOwnCar : EmployeeDriver
{
  public virtual SignedContract Contract { protected get; set; }

  protected override JobResult Handle(RepairJob job)
  {
    ISet<Part> acceptedParts = job.PartsToRepair.ToHashSet();
    acceptedParts.IntersectWith(Contract.CoveredParts);

    var coveredCost = job.EstimatedValue.Percentage(Contract.CoverageRatio.Value);
    var totalCost = job.EstimatedValue - coveredCost;

    return new JobResult(JobResult.Decisions.Accepted).AddParam("totalCost", totalCost)
      .AddParam("acceptedParts", acceptedParts);
  }

}