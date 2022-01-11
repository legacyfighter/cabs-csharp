using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using Microsoft.AspNetCore.Mvc;
using NodaTime;

namespace LegacyFighter.Cabs.Controllers;

[ApiController]
[Route("[controller]")]
public class DriverReportController
{
  private readonly IDriverService _driverService;
  private readonly IDriverRepository _driverRepository;
  private readonly IClaimRepository _claimRepository;
  private readonly IDriverSessionRepository _driverSessionRepository;
  private readonly IClock _clock;
  private readonly ITransactions _transactions;

  public DriverReportController(
    IDriverService driverService,
    IDriverRepository driverRepository,
    IClaimRepository claimRepository,
    IDriverSessionRepository driverSessionRepository,
    IClock clock,
    ITransactions transactions)
  {
    _driverService = driverService;
    _driverRepository = driverRepository;
    _claimRepository = claimRepository;
    _driverSessionRepository = driverSessionRepository;
    _clock = clock;
    _transactions = transactions;
  }

  [HttpGet("/driverreport/{driverId}")]
  public async Task<DriverReport> LoadReportForDriver(long? driverId, [FromQuery] int lastDays)
  {
    await using var tx = await _transactions.BeginTransaction();
    var driverReport = new DriverReport();
    var driverDto = await _driverService.LoadDriver(driverId);
    driverReport.DriverDto = driverDto;
    var driver = await _driverRepository.Find(driverId);
    
    foreach (var attr in driver.Attributes
      .Where(attr=> attr.Name != DriverAttribute.DriverAttributeNames.MedicalExaminationRemarks))
    {
      driverReport.Attributes.Add(new DriverAttributeDto(attr));
    }
    
    var beggingOfToday = _clock.GetCurrentInstant().InZone(DateTimeZoneProviders.Bcl.GetSystemDefault())
      .LocalDateTime.Date.AtStartOfDayInZone(DateTimeZone.Utc).ToInstant();
    var since = beggingOfToday.Minus(Duration.FromDays(lastDays));
    var allByDriverAndLoggedAtAfter =
      await _driverSessionRepository.FindAllByDriverAndLoggedAtAfter(driver, since);
    var sessionsWithTransits = new Dictionary<DriverSessionDto, List<TransitDto>>();
    foreach (var session in allByDriverAndLoggedAtAfter) 
    { 
      var dto = new DriverSessionDto(session);
    var transitsInSession =
        driver.Transits.
          Where(t=>t.Status == Transit.Statuses.Completed &&
                   !(t.CompleteAt < (session.LoggedAt)) &&
                   !(t.CompleteAt > session.LoggedOutAt)).ToList();

      List<TransitDto> transitsDtosInSession = new();
      foreach (var t in transitsInSession) 
      {
        var transitDto = new TransitDto(t);
        var byOwnerAndTransit = await _claimRepository.FindByOwnerAndTransit(t.Client, t);
        if (byOwnerAndTransit.Any())
        {
          var claim = new ClaimDto(byOwnerAndTransit[0]);
          transitDto.ClaimDto = claim;
        }

        transitsDtosInSession.Add(transitDto);
      }
      sessionsWithTransits.Add(dto, transitsDtosInSession);
    }
    driverReport.Sessions = sessionsWithTransits;
    
    await tx.Commit();

    return driverReport;
  }
}