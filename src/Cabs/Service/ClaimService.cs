using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using NodaTime;

namespace LegacyFighter.Cabs.Service;

public class ClaimService : IClaimService
{
  private readonly IClock _clock;
  private readonly IClientRepository _clientRepository;
  private readonly ITransitRepository _transitRepository;
  private readonly IClaimRepository _claimRepository;
  private readonly ClaimNumberGenerator _claimNumberGenerator;
  private readonly IAppProperties _appProperties;
  private readonly IAwardsService _awardsService;
  private readonly IClientNotificationService _clientNotificationService;
  private readonly IDriverNotificationService _driverNotificationService;

  public ClaimService(IClock clock, IClientRepository clientRepository, ITransitRepository transitRepository, IClaimRepository claimRepository, ClaimNumberGenerator claimNumberGenerator, IAppProperties appProperties, IAwardsService awardsService, IClientNotificationService clientNotificationService, IDriverNotificationService driverNotificationService)
  {
    _clock = clock;
    _clientRepository = clientRepository;
    _transitRepository = transitRepository;
    _claimRepository = claimRepository;
    _claimNumberGenerator = claimNumberGenerator;
    _appProperties = appProperties;
    _awardsService = awardsService;
    _clientNotificationService = clientNotificationService;
    _driverNotificationService = driverNotificationService;
  }

  public async Task<Claim> Create(ClaimDto claimDto)
  {
    var claim = new Claim
    {
      CreationDate = _clock.GetCurrentInstant()
    };
    claim.ClaimNo = await _claimNumberGenerator.Generate(claim);
    claim = await Update(claimDto, claim);
    return claim;
  }

  public async Task<Claim> Find(long? id)
  {
    var claim = await _claimRepository.Find(id);
    if (claim == null)
    {
      throw new InvalidOperationException("Claim does not exists");
    }

    return claim;
  }

  public async Task<Claim> Update(ClaimDto claimDto, Claim claim)
  {
    var client = await _clientRepository.Find(claimDto.ClientId);
    var transit = await _transitRepository.Find(claimDto.TransitId);
    if (client == null)
    {
      throw new InvalidOperationException("Client does not exists");
    }

    if (transit == null)
    {
      throw new InvalidOperationException("Transit does not exists");
    }

    if (claimDto.IsDraft)
    {
      claim.Status = Claim.Statuses.Draft;
    }
    else
    {
      claim.Status = Claim.Statuses.New;
    }

    claim.Owner = client;
    claim.Transit = transit;
    claim.CreationDate = _clock.GetCurrentInstant();
    claim.Reason = claimDto.Reason;
    claim.IncidentDescription = claimDto.IncidentDescription;
    return await _claimRepository.Save(claim);
  }

  public async Task<Claim> SetStatus(Claim.Statuses newStatus, long? id)
  {
    var claim = await Find(id);
    claim.Status = newStatus;
    return claim;
  }

  public async Task<Claim> TryToResolveAutomatically(long? id)
  {
    var claim = await Find(id);
    if ((await _claimRepository.FindByOwnerAndTransit(claim.Owner, claim.Transit)).Count > 1)
    {
      claim.Status = Claim.Statuses.Escalated;
      claim.CompletionDate = SystemClock.Instance.GetCurrentInstant();
      claim.ChangeDate = SystemClock.Instance.GetCurrentInstant();
      claim.CompletionMode = Claim.CompletionModes.Manual;
      return claim;
    }

    if ((await _claimRepository.FindByOwner(claim.Owner)).Count <= 3)
    {
      claim.Status = Claim.Statuses.Refunded;
      claim.CompletionDate = SystemClock.Instance.GetCurrentInstant();
      claim.ChangeDate = SystemClock.Instance.GetCurrentInstant();
      claim.CompletionMode = Claim.CompletionModes.Automatic;
      _clientNotificationService.NotifyClientAboutRefund(claim.ClaimNo, claim.Owner.Id);
      return claim;
    }

    if (claim.Owner.Type == Client.Types.Vip)
    {
      if (claim.Transit.Price < _appProperties.AutomaticRefundForVipThreshold)
      {
        claim.Status = Claim.Statuses.Refunded;
        claim.CompletionDate = SystemClock.Instance.GetCurrentInstant();
        claim.ChangeDate = SystemClock.Instance.GetCurrentInstant();
        claim.CompletionMode = Claim.CompletionModes.Automatic;
        _clientNotificationService.NotifyClientAboutRefund(claim.ClaimNo, claim.Owner.Id);
        await _awardsService.RegisterSpecialMiles(claim.Owner.Id, 10);
      }
      else
      {
        claim.Status = Claim.Statuses.Escalated;
        claim.CompletionDate = SystemClock.Instance.GetCurrentInstant();
        claim.ChangeDate = SystemClock.Instance.GetCurrentInstant();
        claim.CompletionMode = Claim.CompletionModes.Manual;
        _driverNotificationService.AskDriverForDetailsAboutClaim(claim.ClaimNo,
          claim.Transit.Driver.Id);
      }
    }
    else
    {
      if ((await _transitRepository.FindByClient(claim.Owner)).Count >=
           _appProperties.NoOfTransitsForClaimAutomaticRefund)
      {
        if (claim.Transit.Price < _appProperties.AutomaticRefundForVipThreshold)
        {
          claim.Status = Claim.Statuses.Refunded;
          claim.CompletionDate = SystemClock.Instance.GetCurrentInstant();
          claim.ChangeDate = SystemClock.Instance.GetCurrentInstant();
          claim.CompletionMode = Claim.CompletionModes.Automatic;
          _clientNotificationService.NotifyClientAboutRefund(claim.ClaimNo, claim.Owner.Id);
        }
        else
        {
          claim.Status = Claim.Statuses.Escalated;
          claim.CompletionDate = SystemClock.Instance.GetCurrentInstant();
          claim.ChangeDate = SystemClock.Instance.GetCurrentInstant();
          claim.CompletionMode = Claim.CompletionModes.Manual;
          _clientNotificationService.AskForMoreInformation(claim.ClaimNo, claim.Owner.Id);
        }
      }
      else
      {
        claim.Status = Claim.Statuses.Escalated;
        claim.CompletionDate = SystemClock.Instance.GetCurrentInstant();
        claim.ChangeDate = SystemClock.Instance.GetCurrentInstant();
        claim.CompletionMode = Claim.CompletionModes.Manual;
        _driverNotificationService.AskDriverForDetailsAboutClaim(claim.ClaimNo, claim.Owner.Id);
      }
    }

    return claim;
  }
}