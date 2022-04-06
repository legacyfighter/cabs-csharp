using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using LegacyFighter.Cabs.TransitDetail;
using NodaTime;

namespace LegacyFighter.Cabs.Crm.Claims;

public class ClaimService : IClaimService
{
  private readonly IClock _clock;
  private readonly IClientRepository _clientRepository;
  private readonly ITransitDetailsFacade _transitDetailsFacade;
  private readonly IClaimRepository _claimRepository;
  private readonly ClaimNumberGenerator _claimNumberGenerator;
  private readonly IAppProperties _appProperties;
  private readonly IAwardsService _awardsService;
  private readonly IClientNotificationService _clientNotificationService;
  private readonly IDriverNotificationService _driverNotificationService;
  private readonly IClaimsResolverRepository _claimsResolverRepository;

  public ClaimService(IClock clock,
    IClientRepository clientRepository,
    ITransitDetailsFacade transitDetailsFacade,
    IClaimRepository claimRepository,
    ClaimNumberGenerator claimNumberGenerator,
    IAppProperties appProperties,
    IAwardsService awardsService,
    IClientNotificationService clientNotificationService,
    IDriverNotificationService driverNotificationService,
    IClaimsResolverRepository claimsResolverRepository)
  {
    _clock = clock;
    _clientRepository = clientRepository;
    _transitDetailsFacade = transitDetailsFacade;
    _claimRepository = claimRepository;
    _claimNumberGenerator = claimNumberGenerator;
    _appProperties = appProperties;
    _awardsService = awardsService;
    _clientNotificationService = clientNotificationService;
    _driverNotificationService = driverNotificationService;
    _claimsResolverRepository = claimsResolverRepository;
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
    var transit = await _transitDetailsFacade.Find(claimDto.TransitId);
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
      claim.Status = Statuses.Draft;
    }
    else
    {
      claim.Status = Statuses.New;
    }

    claim.OwnerId = client.Id;
    claim.TransitId = transit.TransitId;
    claim.TransitPrice = transit.Price;
    claim.CreationDate = _clock.GetCurrentInstant();
    claim.Reason = claimDto.Reason;
    claim.IncidentDescription = claimDto.IncidentDescription;
    return await _claimRepository.Save(claim);
  }

  public async Task<Claim> SetStatus(Statuses newStatus, long? id)
  {
    var claim = await Find(id);
    claim.Status = newStatus;
    return claim;
  }

  public async Task<Claim> TryToResolveAutomatically(long? id)
  {
    var claim = await Find(id);

    var claimsResolver = await FindOrCreateResolver(claim.OwnerId);
    var transitsDoneByClient = await _transitDetailsFacade.FindByClient(claim.OwnerId);
    var clientType = (await _clientRepository.Find(claim.OwnerId)).Type;
    var result = claimsResolver.Resolve(claim, clientType, _appProperties.AutomaticRefundForVipThreshold,
      transitsDoneByClient.Count, _appProperties.NoOfTransitsForClaimAutomaticRefund);

    if (result.Decision == Statuses.Refunded)
    {
      claim.Refund();
      _clientNotificationService.NotifyClientAboutRefund(claim.ClaimNo, claim.OwnerId);
      if (clientType == Client.Types.Vip)
      {
        await _awardsService.RegisterNonExpiringMiles(claim.OwnerId, 10);
      }
    }

    if (result.Decision == Statuses.Escalated)
    {
      claim.Escalate();
    }

    if (result.WhoToAsk == ClaimsResolver.WhoToAsk.AskDriver)
    {
      var transitDetailsDto = await _transitDetailsFacade.Find(claim.TransitId);
      _driverNotificationService.AskDriverForDetailsAboutClaim(claim.ClaimNo, transitDetailsDto.DriverId);
    }

    if (result.WhoToAsk == ClaimsResolver.WhoToAsk.AskClient)
    {
      _clientNotificationService.AskForMoreInformation(claim.ClaimNo, claim.OwnerId);
    }

    return claim;
  }

  private async Task<ClaimsResolver> FindOrCreateResolver(long? clientId)
  {
    var resolver = 
      await _claimsResolverRepository.FindByClientId(clientId) 
      ?? await _claimsResolverRepository.SaveAsync(new ClaimsResolver(clientId));

    return resolver;
  }

  public async Task<int> GetNumberOfClaims(long? clientId) 
  {
    return (await _claimRepository.FindAllByOwnerId(clientId)).Count;
  }
}