using LegacyFighter.Cabs.Parties.Api;
using LegacyFighter.Cabs.Parties.Model.Parties;
using LegacyFighter.Cabs.Repair.Model.Dict;

namespace LegacyFighter.Cabs.Repair.Api;

public class ContractManager : IContractManager
{
  private readonly IPartyRepository _partyRepository;
  private readonly IPartyRelationshipRepository _partyRelationshipRepository;

  public ContractManager(
    IPartyRepository partyRepository, 
    IPartyRelationshipRepository partyRelationshipRepository)
  {
    _partyRepository = partyRepository;
    _partyRelationshipRepository = partyRelationshipRepository;
  }

  public async Task ExtendedWarrantyContractSigned(PartyId insurerId, PartyId vehicleId)
  {
    var insurer = await _partyRepository.Save(insurerId.ToGuid());
    var vehicle = await _partyRepository.Save(vehicleId.ToGuid());

    await _partyRelationshipRepository.Save(PartyRelationshipsDictionary.Repair.ToString(),
      PartyRolesDictionary.Insurer.RoleName, insurer,
      PartyRolesDictionary.Insured.RoleName, vehicle);
  }

  public async Task ManufacturerWarrantyRegistered(PartyId distributorId, PartyId vehicleId)
  {
    var distributor = await _partyRepository.Save(distributorId.ToGuid());
    var vehicle = await _partyRepository.Save(vehicleId.ToGuid());

    await _partyRelationshipRepository.Save(PartyRelationshipsDictionary.Repair.ToString(),
      PartyRolesDictionary.Guarantor.RoleName, distributor,
      PartyRolesDictionary.Customer.RoleName, vehicle);
  }
}