namespace LegacyFighter.Cabs.Ride;

public class StartTransitService : IStartTransitService
{
  private readonly IRequestTransitService _requestTransitService;
  private readonly ITransitRepository _transitRepository;

  public StartTransitService(
    IRequestTransitService requestTransitService,
    ITransitRepository transitRepository)
  {
    _requestTransitService = requestTransitService;
    _transitRepository = transitRepository;
  }

  public async Task<Transit> Start(Guid requestGuid)
  {
    var transit = new Transit(
      await _requestTransitService.FindTariff(requestGuid),
      requestGuid);
    return await _transitRepository.Save(transit);
  }
}