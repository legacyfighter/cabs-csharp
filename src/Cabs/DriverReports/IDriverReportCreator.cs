using LegacyFighter.Cabs.Dto;

namespace LegacyFighter.Cabs.DriverReports;

public interface IDriverReportCreator
{
  Task<DriverReport> CreateReport(long? driverId, int days);
}