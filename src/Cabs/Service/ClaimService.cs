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
  private readonly IClaimsResolverRepository _claimsResolverRepository;

  public ClaimService(IClock clock,
    IClientRepository clientRepository,
    ITransitRepository transitRepository,
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
    _transitRepository = transitRepository;
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

    var claimsResolver = await FindOrCreateResolver(claim.Owner);
    var transitsDoneByClient = await _transitRepository.FindByClient(claim.Owner);
    var result = claimsResolver.Resolve(claim, _appProperties.AutomaticRefundForVipThreshold,
      transitsDoneByClient.Count, _appProperties.NoOfTransitsForClaimAutomaticRefund);

    if (result.Decision == Claim.Statuses.Refunded)
    {
      claim.Refund();
      _clientNotificationService.NotifyClientAboutRefund(claim.ClaimNo, claim.Owner.Id);
      if (claim.Owner.Type == Client.Types.Vip)
      {
        await _awardsService.RegisterSpecialMiles(claim.Owner.Id, 10);
      }
    }

    if (result.Decision == Claim.Statuses.Escalated)
    {
      claim.Escalate();
    }

    if (result.WhoToAsk == ClaimsResolver.WhoToAsk.AskDriver)
    {
      _driverNotificationService.AskDriverForDetailsAboutClaim(claim.ClaimNo, claim.Transit.Driver.Id);
    }

    if (result.WhoToAsk == ClaimsResolver.WhoToAsk.AskClient)
    {
      _clientNotificationService.AskForMoreInformation(claim.ClaimNo, claim.Owner.Id);
    }

    return claim;
  }

  private async Task<ClaimsResolver> FindOrCreateResolver(Client client)
  {
    var resolver = 
      await _claimsResolverRepository.FindByClientId(client.Id) 
      ?? await _claimsResolverRepository.SaveAsync(new ClaimsResolver(client.Id));

    return resolver;
  }
}