using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.Dto;

namespace LegacyFighter.Cabs.DriverReports;

public class DriverReportCreator
{
  private readonly IDriverReportCreator _sqlBasedDriverReportCreator;
  private readonly IDriverReportCreator _oldDriverReportCreator;
  private readonly FeatureFlags _featureFlags;

  public DriverReportCreator(
    IDriverReportCreator sqlBasedDriverReportCreator,
    IDriverReportCreator oldDriverReportCreator, 
    FeatureFlags featureFlags)
  {
    _sqlBasedDriverReportCreator = sqlBasedDriverReportCreator;
    _oldDriverReportCreator = oldDriverReportCreator;
    _featureFlags = featureFlags;
  }

  public async Task<DriverReport> Create(long? driverId, int days)
  {
    if (await ShouldUseNewReport())
    {
      return await _sqlBasedDriverReportCreator.CreateReport(driverId, days);
    }

    return await _oldDriverReportCreator.CreateReport(driverId, days);
  }

  private async Task<bool> ShouldUseNewReport()
  {
    return await _featureFlags.IsDriverReportSqlActive();
  }
}