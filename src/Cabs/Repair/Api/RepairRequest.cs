using LegacyFighter.Cabs.Parties.Api;
using LegacyFighter.Cabs.Repair.Legacy.Parts;

namespace LegacyFighter.Cabs.Repair.Api;

public class RepairRequest
{
  public RepairRequest(PartyId vehicle, ISet<Part> parts)
  {
    Vehicle = vehicle;
    PartsToRepair = parts;
  }

  public ISet<Part> PartsToRepair { get; }

  public PartyId Vehicle { get; }
}