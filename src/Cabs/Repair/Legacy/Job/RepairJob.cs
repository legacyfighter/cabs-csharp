using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Repair.Legacy.Parts;

namespace LegacyFighter.Cabs.Repair.Legacy.Job;

public class RepairJob : CommonBaseAbstractJob
{
  public Money EstimatedValue { get; set; }
  public ISet<Part> PartsToRepair { get; set; }
}