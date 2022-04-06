namespace LegacyFighter.Cabs.Ride;

public class DemandService : IDemandService
{
  private readonly ITransitDemandRepository _transitDemandRepository;

  public DemandService(ITransitDemandRepository transitDemandRepository)
  {
    _transitDemandRepository = transitDemandRepository;
  }

  public async Task PublishDemand(Guid requestGuid)
  {
    await _transitDemandRepository.Save(new TransitDemand(requestGuid));
  }

  public async Task CancelDemand(Guid requestGuid)
  {
    var transitDemand = await _transitDemandRepository.FindByTransitRequestGuid(requestGuid);
    if (transitDemand != null)
    {
      transitDemand.Cancel();
    }
  }

  public async Task AcceptDemand(Guid requestGuid)
  {
    var transitDemand = await _transitDemandRepository.FindByTransitRequestGuid(requestGuid);
    if (transitDemand != null)
    {
      transitDemand.Accept();
    }
  }

  public async Task<bool> ExistsFor(Guid requestGuid)
  {
    return await _transitDemandRepository.FindByTransitRequestGuid(requestGuid) != null;
  }
}