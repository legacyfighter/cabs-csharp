using LegacyFighter.Cabs.Ride.Details;

namespace LegacyFighter.Cabs.Crm.TransitAnalyzer;

public class PopulateGraphService : IPopulateGraphService
{
  private readonly GraphTransitAnalyzer _graphTransitAnalyzer;
  private readonly ITransitDetailsFacade _transitDetailsFacade;

  public PopulateGraphService(
    GraphTransitAnalyzer graphTransitAnalyzer,
    ITransitDetailsFacade transitDetailsFacade)
  {
    _graphTransitAnalyzer = graphTransitAnalyzer;
    _transitDetailsFacade = transitDetailsFacade;
  }

  public async Task Populate()
  {
    foreach (var transit in await _transitDetailsFacade.FindCompleted())
    {
      await AddToGraph(transit);
    }
  }

  private async Task AddToGraph(TransitDetailsDto transitDetails)
  {
    var clientId = transitDetails.Client.Id;
    await _graphTransitAnalyzer.AddTransitBetweenAddresses(
      clientId,
      transitDetails.TransitId,
      transitDetails.From.Hash,
      transitDetails.To.Hash,
      transitDetails.Started!.Value,
      transitDetails.CompletedAt!.Value);
  }
}