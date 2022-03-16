using System.Linq;
using LegacyFighter.Cabs.Parties.Api;
using LegacyFighter.Cabs.Repair.Api;
using LegacyFighter.Cabs.Repair.Legacy.Parts;
using LegacyFighter.CabsTests.Common;

namespace LegacyFighter.CabsTests.Repair.Api;

public class RepairProcessTest
{
  private PartyId _vehicle = default!;
  private PartyId _handlingParty = default!;
  private CabsApp _app = default!;
  private RepairProcess VehicleRepairProcess => _app.VehicleRepairProcess;
  private IContractManager ContractManager => _app.ContractManager;

  [SetUp]
  public void InitializeApp()
  {
    _vehicle = new PartyId();
    _handlingParty = new PartyId();
    _app = CabsApp.CreateInstance();
  }

  [TearDown]
  public async Task DisposeOfApp()
  {
    await _app.DisposeAsync();
  }

  [Test]
  public async Task WarrantyByInsuranceCoversAllButPaint()
  {
    //given
    await ContractManager.ExtendedWarrantyContractSigned(_handlingParty, _vehicle);

    var parts = new[] { Part.Engine, Part.Gearbox, Part.Paint, Part.Suspension }.ToHashSet();
    var repairRequest = new RepairRequest(_vehicle, parts);
    //when
    var result = await VehicleRepairProcess.Resolve(repairRequest);
    //then
    new VehicleRepairAssert(result).By(_handlingParty).Free().AllPartsBut(parts, new[] { Part.Paint });
  }

  [Test]
  public async Task ManufacturerWarrantyCoversAll()
  {
    //given
    await ContractManager.ManufacturerWarrantyRegistered(_handlingParty, _vehicle);

    var parts = new[] { Part.Engine, Part.Gearbox, Part.Paint, Part.Suspension }.ToHashSet();
    var repairRequest = new RepairRequest(_vehicle, parts);
    //when
    var result = await VehicleRepairProcess.Resolve(repairRequest);
    //then
    new VehicleRepairAssert(result).By(_handlingParty).Free().AllParts(parts);
  }
}