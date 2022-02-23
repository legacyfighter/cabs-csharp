using Microsoft.FeatureManagement;

namespace LegacyFighter.Cabs.Config;

public class FeatureFlags
{
  public const string DriverReportSql = "DriverReportSql";
  private readonly IFeatureManager _featureManager;

  public FeatureFlags(IFeatureManager featureManager)
  {
    _featureManager = featureManager;
  }

  public async Task<bool> IsDriverReportSqlActive()
  {
    return await _featureManager.IsEnabledAsync(DriverReportSql);
  }
}