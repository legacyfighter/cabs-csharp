using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Repair.Legacy.Parts;

namespace LegacyFighter.Cabs.Repair.Legacy.User;

public class SignedContract : BaseEntity
{
  public virtual ISet<Part> CoveredParts { get; set; }
  public double? CoverageRatio { get; set; }
}