using LegacyFighter.Cabs.Common;
using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.DriverFleet.DriverReports;

[ApiController]
[Route("[controller]")]
public class DriverReportController
{
  private readonly SqlBasedDriverReportCreator _driverReportCreator;
  private readonly ITransactions _transactions; 

  public DriverReportController(
    SqlBasedDriverReportCreator driverReportCreator, 
    ITransactions transactions)
  {
    _driverReportCreator = driverReportCreator;
    _transactions = transactions;
  }

  [HttpGet("/driverreport/{driverId}")]
  public async Task<DriverReport> LoadReportForDriver(long? driverId, [FromQuery] int lastDays)
  {
    await using var tx = await _transactions.BeginTransaction();
    var driverReport = await _driverReportCreator.CreateReport(driverId, lastDays);
    await tx.Commit();

    return driverReport;
  }
}