using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using NodaTime;

namespace LegacyFighter.Cabs.DriverReports;

internal class OldDriverReportCreator : IDriverReportCreator
{
  private readonly IDriverService _driverService;
  private readonly IDriverRepository _driverRepository;
  private readonly IClock _clock;
  private readonly IDriverSessionRepository _driverSessionRepository;
  private readonly IClaimRepository _claimRepository;

  public OldDriverReportCreator(
    IDriverService driverService,
    IDriverRepository driverRepository,
    IClock clock,
    IDriverSessionRepository driverSessionRepository,
    IClaimRepository claimRepository)
  {
    _driverService = driverService;
    _driverRepository = driverRepository;
    _clock = clock;
    _driverSessionRepository = driverSessionRepository;
    _claimRepository = claimRepository;
  }

  public async Task<DriverReport> CreateReport(long? driverId, int lastDays)
  {
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
                   !(t.CompleteAt < session.LoggedAt) &&
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
    return driverReport;
  }

}