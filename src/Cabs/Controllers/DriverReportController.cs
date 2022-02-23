using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Dto;
using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.Controllers;

[ApiController]
[Route("[controller]")]
public class DriverReportController
{
  private readonly SqlBasedDriverReportCreator _sqlBasedDriverReportCreator;
  private readonly ITransactions _transactions; 

  public DriverReportController(
    SqlBasedDriverReportCreator sqlBasedDriverReportCreator, 
    ITransactions transactions)
  {
    _sqlBasedDriverReportCreator = sqlBasedDriverReportCreator;
    _transactions = transactions;
  }

  [HttpGet("/driverreport/{driverId}")]
  public async Task<DriverReport> LoadReportForDriver(long? driverId, [FromQuery] int lastDays)
  {
    await using var tx = await _transactions.BeginTransaction();
    var driverReport = await _sqlBasedDriverReportCreator.CreateReport(driverId, lastDays);
    await tx.Commit();

    return driverReport;
  }
}