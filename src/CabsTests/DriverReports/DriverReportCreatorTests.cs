using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.DriverReports;
using Microsoft.FeatureManagement;

namespace LegacyFighter.CabsTests.DriverReports;

public class DriverReportCreatorTests
{
  private const int LastDays = 3;
  private const long DriverId = 1L;

  private DriverReportCreator _driverReportCreator = default!;
  private IDriverReportCreator _oldDriverReportCreator = default!;
  private IDriverReportCreator _sqlBasedDriverReportCreator = default!;
  private IFeatureManager _featureManager = default!;

  [SetUp]
  public void SetUp()
  {
    _oldDriverReportCreator = Substitute.For<IDriverReportCreator>();
    _sqlBasedDriverReportCreator = Substitute.For<IDriverReportCreator>();
    _featureManager = Substitute.For<IFeatureManager>();
    _driverReportCreator = new DriverReportCreator(
      _sqlBasedDriverReportCreator, 
      _oldDriverReportCreator, 
      new FeatureFlags(_featureManager));
  }

  [Test]
  public async Task CallsNewReport()
  {
    _featureManager.IsEnabledAsync(FeatureFlags.DriverReportSql).Returns(true);
    //when
    await _driverReportCreator.Create(DriverId, LastDays);

    //then
    await _sqlBasedDriverReportCreator.Received(1).CreateReport(DriverId, LastDays);
  }

  [Test]
  public async Task CallsOldReport() {
    _featureManager.IsEnabledAsync(FeatureFlags.DriverReportSql).Returns(false);
    
    //when
    await _driverReportCreator.Create(DriverId, LastDays);

    //then
    await _oldDriverReportCreator.Received(1).CreateReport(DriverId, LastDays);
  }

}