using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.TransitDetail;

namespace LegacyFighter.Cabs.TransitAnalyzer;

public class PopulateGraphService : IPopulateGraphService
{
  private readonly ITransitRepository _transitRepository;
  private readonly GraphTransitAnalyzer _graphTransitAnalyzer;
  private readonly ITransitDetailsFacade _transitDetailsFacade;

  public PopulateGraphService(
    ITransitRepository transitRepository,
    GraphTransitAnalyzer graphTransitAnalyzer,
    ITransitDetailsFacade transitDetailsFacade)
  {
    _transitRepository = transitRepository;
    _graphTransitAnalyzer = graphTransitAnalyzer;
    _transitDetailsFacade = transitDetailsFacade;
  }

  public async Task Populate()
  {
    foreach (var transit in await _transitRepository.FindAllByStatus(Transit.Statuses.Completed))
    {
      await AddToGraph(transit);
    }
  }

  private async Task AddToGraph(Transit transit)
  {
    var transitDetails = await _transitDetailsFacade.Find(transit.Id);
    var clientId = transitDetails.Client.Id;
    await _graphTransitAnalyzer.AddTransitBetweenAddresses(
      clientId,
      transit.Id,
      transitDetails.From.Hash,
      transitDetails.To.Hash,
      transitDetails.Started!.Value,
      transitDetails.CompletedAt!.Value);
  }
}