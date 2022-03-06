using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.TransitAnalyzer;

public class PopulateGraphService : IPopulateGraphService
{
  private readonly ITransitRepository _transitRepository;
  private readonly GraphTransitAnalyzer _graphTransitAnalyzer;

  public PopulateGraphService(ITransitRepository transitRepository, GraphTransitAnalyzer graphTransitAnalyzer)
  {
    _transitRepository = transitRepository;
    _graphTransitAnalyzer = graphTransitAnalyzer;
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
    var clientId = transit.Client.Id;
    await _graphTransitAnalyzer.AddTransitBetweenAddresses(
      clientId,
      transit.Id,
      transit.From.Hash,
      transit.To.Hash,
      transit.Started!.Value,
      transit.CompleteAt!.Value);
  }
}