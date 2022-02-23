using LegacyFighter.Cabs.Common;
using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.DriverReports;

[ApiController]
[Route("[controller]")]
public class DriverReportController
{
  private readonly DriverReportCreator _driverReportCreator;
  private readonly ITransactions _transactions; 

  public DriverReportController(
    DriverReportCreator driverReportCreator, 
    ITransactions transactions)
  {
    _driverReportCreator = driverReportCreator;
    _transactions = transactions;
  }

  [HttpGet("/driverreport/{driverId}")]
  public async Task<Dto.DriverReport> LoadReportForDriver(long? driverId, [FromQuery] int lastDays)
  {
    await using var tx = await _transactions.BeginTransaction();
    var driverReport = await _driverReportCreator.Create(driverId, lastDays);
    await tx.Commit();

    return driverReport;
  }
}