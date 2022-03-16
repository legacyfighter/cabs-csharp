using LegacyFighter.Cabs.Parties.Api;

namespace LegacyFighter.Cabs.Repair.Api;

public interface IContractManager
{
  Task ExtendedWarrantyContractSigned(PartyId insurerId, PartyId vehicleId);
  Task ManufacturerWarrantyRegistered(PartyId distributorId, PartyId vehicleId);
}