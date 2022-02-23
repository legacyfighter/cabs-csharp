using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.DriverReports;
using LegacyFighter.Cabs.Dto;
using Microsoft.FeatureManagement;
using TddXt.XNSubstitute;

namespace LegacyFighter.CabsTests.DriverReports;

public class DriverReportCreatorTests
{
  private const int LastDays = 3;
  private const long DriverId = 1L;

  private DriverReportCreator _reportCreator = default!;
  private IDriverReportCreator _oldDriverReportCreator = default!;
  private IDriverReportCreator _sqlBasedDriverReportCreator = default!;
  private IFeatureManager _testFeatureManager = default!;
  private DriverReport _sqlReport = default!;
  private DriverReport _oldReport = default!;
  private IDriverReportReconciliation _driverReportReconciliation = default!;

  [SetUp]
  public void SetUp()
  {
    _sqlReport = new DriverReport();
    _oldReport = new DriverReport();
    _oldDriverReportCreator = Substitute.For<IDriverReportCreator>();
    _sqlBasedDriverReportCreator = Substitute.For<IDriverReportCreator>();
    _testFeatureManager = Substitute.For<IFeatureManager>();
    _driverReportReconciliation = Substitute.For<IDriverReportReconciliation>();
    _reportCreator = new DriverReportCreator(
      _sqlBasedDriverReportCreator,
      _oldDriverReportCreator,
      new FeatureFlags(_testFeatureManager),
      _driverReportReconciliation);
  }

  [Test]
  public async Task CallsNewReport()
  {
    //given
    NewSqlWayReturnsReport();
    _testFeatureManager.IsEnabledAsync(FeatureFlags.DriverReportCreationReconciliation).Returns(false);
    _testFeatureManager.IsEnabledAsync(FeatureFlags.DriverReportSql).Returns(true);
    
    //when
    await _reportCreator.Create(DriverId, LastDays);

    //then
    await _sqlBasedDriverReportCreator.Received(1).CreateReport(DriverId, LastDays);
    _oldDriverReportCreator.ReceivedNothing();
  }

  [Test]
  public async Task CallsOldReport()
  {
    //given
    OldWayReturnsReport();
    _testFeatureManager.IsEnabledAsync(FeatureFlags.DriverReportCreationReconciliation).Returns(false);
    _testFeatureManager.IsEnabledAsync(FeatureFlags.DriverReportSql).Returns(false);

    //when
    await _reportCreator.Create(DriverId, LastDays);

    //then
    await _oldDriverReportCreator.Received(1).CreateReport(DriverId, LastDays);
    _sqlBasedDriverReportCreator.ReceivedNothing();
  }

  [Test]
  public async Task CallsReconciliationAndUsesOldReport()
  {
    //given
    BothWaysReturnReport();
    _testFeatureManager.IsEnabledAsync(FeatureFlags.DriverReportCreationReconciliation).Returns(true);
    _testFeatureManager.IsEnabledAsync(FeatureFlags.DriverReportSql).Returns(false);

    //when
    await _reportCreator.Create(DriverId, LastDays);

    //then
    await _oldDriverReportCreator.Received(1).CreateReport(DriverId, LastDays);
    await _sqlBasedDriverReportCreator.Received(1).CreateReport(DriverId, LastDays);
    _driverReportReconciliation.Received(1).Compare(_oldReport, _sqlReport);
  }

  [Test]
  public async Task CallsReconciliationAndUsesNewReport()
  {
    //given
    BothWaysReturnReport();
    _testFeatureManager.IsEnabledAsync(FeatureFlags.DriverReportCreationReconciliation).Returns(true);
    _testFeatureManager.IsEnabledAsync(FeatureFlags.DriverReportSql).Returns(true);

    //when
    await _reportCreator.Create(DriverId, LastDays);

    //then
    await _sqlBasedDriverReportCreator.Received(1).CreateReport(DriverId, LastDays);
    await _oldDriverReportCreator.Received(1).CreateReport(DriverId, LastDays);
    _driverReportReconciliation.Received(1).Compare(_oldReport, _sqlReport);
  }

  private void BothWaysReturnReport()
  {
    OldWayReturnsReport();
    NewSqlWayReturnsReport();
  }

  private void NewSqlWayReturnsReport()
  {
    _sqlBasedDriverReportCreator.CreateReport(DriverId, LastDays).Returns(_sqlReport);
  }

  private void OldWayReturnsReport()
  {
    _oldDriverReportCreator.CreateReport(DriverId, LastDays).Returns(_oldReport);
  }


}