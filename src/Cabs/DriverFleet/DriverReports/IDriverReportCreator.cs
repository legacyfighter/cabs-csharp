using LegacyFighter.Cabs.Dto;

namespace LegacyFighter.Cabs.DriverFleet.DriverReports;

public interface IDriverReportCreator
{
  Task<DriverReport> CreateReport(long? driverId, int days);
}