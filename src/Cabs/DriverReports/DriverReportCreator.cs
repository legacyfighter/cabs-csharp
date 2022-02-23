using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.Dto;

namespace LegacyFighter.Cabs.DriverReports;

public class DriverReportCreator
{
  private readonly IDriverReportCreator _sqlBasedDriverReportCreator;
  private readonly IDriverReportCreator _oldDriverReportCreator;
  private readonly FeatureFlags _featureFlags;
  private readonly IDriverReportReconciliation _driverReportReconciliation;

  public DriverReportCreator(
    IDriverReportCreator sqlBasedDriverReportCreator,
    IDriverReportCreator oldDriverReportCreator,
    FeatureFlags featureFlags,
    IDriverReportReconciliation driverReportReconciliation)
  {
    _sqlBasedDriverReportCreator = sqlBasedDriverReportCreator;
    _oldDriverReportCreator = oldDriverReportCreator;
    _featureFlags = featureFlags;
    _driverReportReconciliation = driverReportReconciliation;
  }

  public async Task<DriverReport> Create(long? driverId, int days)
  {
    DriverReport newReport = null;
    DriverReport oldReport = null;
    if (await ShouldCompare())
    {
      newReport = await _sqlBasedDriverReportCreator.CreateReport(driverId, days);
      oldReport = await _oldDriverReportCreator.CreateReport(driverId, days);
      _driverReportReconciliation.Compare(oldReport, newReport);
    }

    if (await ShouldUseNewReport())
    {
      if (newReport == null)
      {
        newReport = await _sqlBasedDriverReportCreator.CreateReport(driverId, days);
      }

      return newReport;
    }

    if (oldReport == null)
    {
      oldReport = await _oldDriverReportCreator.CreateReport(driverId, days);
    }

    return oldReport;
  }

  private async Task<bool> ShouldCompare()
  {
    return await _featureFlags.IsDriverReportCreationReconciliationActive();
  }

  private async Task<bool> ShouldUseNewReport()
  {
    return await _featureFlags.IsDriverReportSqlActive();
  }
}

public interface IDriverReportReconciliation
{
  void Compare(DriverReport oldOne, DriverReport newOne);
}

internal class TestDummyReconciliation : IDriverReportReconciliation
{
  public void Compare(DriverReport oldOne, DriverReport newOne)
  {
    //noop
  }
}