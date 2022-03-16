using Core.Maybe;
using LegacyFighter.Cabs.Parties.Api;
using LegacyFighter.Cabs.Repair.Model.Dict;
using LegacyFighter.Cabs.Repair.Model.Roles.Repair;

namespace LegacyFighter.Cabs.Repair.Api;

public class RepairProcess
{
  private readonly PartyMapper _partyMapper;

  public RepairProcess(PartyMapper partyMapper)
  {
    _partyMapper = partyMapper;
  }

  public async Task<ResolveResult> Resolve(RepairRequest repairRequest)
  {
    return (await _partyMapper.MapRelation(repairRequest.Vehicle, PartyRelationshipsDictionary.Repair.ToString()))
      .Select(RoleObjectFactory.From)
      .SelectMany(rof => rof.GetRole<RoleForRepairer>())
      .Select(role => role.Handle(repairRequest))
      .Select(repairingResult => new ResolveResult(ResolveResult.Statuses.Success, repairingResult.HandlingParty,
        repairingResult.TotalCost, repairingResult.HandledParts))
      .OrElse(() => new ResolveResult(ResolveResult.Statuses.Error));
  }

  public async Task<ResolveResult> ResolveOldSchoolVersion(RepairRequest repairRequest)
  {
    //who is responsible for repairing the vehicle
    var relationship =
      await _partyMapper.MapRelation(repairRequest.Vehicle, PartyRelationshipsDictionary.Repair.ToString());
    if (relationship.HasValue)
    {
      var roleObjectFactory = RoleObjectFactory.From(relationship.Value());
      //dynamically assigned rules
      var role = roleObjectFactory.GetRole<RoleForRepairer>();
      if (role.HasValue)
      {
        //actual repair request handling
        var repairingResult = role.Value().Handle(repairRequest);
        return new ResolveResult(ResolveResult.Statuses.Success, repairingResult.HandlingParty,
          repairingResult.TotalCost, repairingResult.HandledParts);
      }
    }

    return new ResolveResult(ResolveResult.Statuses.Error);
  }
}