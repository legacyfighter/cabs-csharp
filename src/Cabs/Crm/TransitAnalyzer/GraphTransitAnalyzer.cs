using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.Entity.Events;
using MediatR;
using Microsoft.Extensions.Options;
using Neo4j.Driver;
using NodaTime;

namespace LegacyFighter.Cabs.Crm.TransitAnalyzer;

public class GraphTransitAnalyzer : INotificationHandler<TransitCompleted>
{
  private readonly IDriver _driver;
  private readonly string _addressNodeName;
  private readonly string _transitNodeName;

  public GraphTransitAnalyzer(
    IDriver driver, 
    IOptions<GraphDatabaseOptions> options)
  {
    _driver = driver;
    _addressNodeName = options.Value.AddressNodeName ?? "Address";
    _transitNodeName = options.Value.TransitNodeName ?? "Transit";
  }

  public async Task<List<long?>> Analyze(long? clientId, int? addressHash)
  {
    await using var session = _driver.AsyncSession();
    await using var t = await session.BeginTransactionAsync();
    var result = await t.RunAsync(
      $"MATCH p=(a:{_addressNodeName})-[:{_transitNodeName}*]->(:{_addressNodeName}) WHERE a.hash = {addressHash} " +
      $"AND (ALL(x IN range(1, length(p)-1) WHERE ((relationships(p)[x]).clientId = {clientId}) " +
      "AND 0 <= duration.inSeconds( (relationships(p)[x-1]).completeAt, (relationships(p)[x]).started).minutes <= 15)) " +
      "AND length(p) >= 1 " +
      "RETURN [x in nodes(p) | x.hash] AS hashes " +
      "ORDER BY length(p) DESC LIMIT 1");
    var hashes = ((List<object>)(await result.ToListAsync())[0].Values["hashes"]).Cast<long?>().ToList();
    await t.CommitAsync();
    return hashes;
  }

  async Task INotificationHandler<TransitCompleted>.Handle(
    TransitCompleted transitCompleted, 
    CancellationToken cancellationToken)
  {
    await AddTransitBetweenAddresses(
      transitCompleted.ClientId,
      transitCompleted.TransitId,
      transitCompleted.AddressFromHash,
      transitCompleted.AddressToHash,
      transitCompleted.Started,
      transitCompleted.CompleteAt);
  }

  public async Task AddTransitBetweenAddresses(
    long? clientId,
    long? transitId,
    int? addressFromHash,
    int? addressToHash,
    Instant started,
    Instant completeAt)
  {
    await using var session = _driver.AsyncSession();
    await using var t = await session.BeginTransactionAsync();
    await t.RunAsync($"MERGE (from:{_addressNodeName} {{hash: {addressFromHash}}})");
    await t.RunAsync($"MERGE (to:{_addressNodeName} {{hash: {addressToHash}}})");
    await t.RunAsync("MATCH " +
                     $"(from:{_addressNodeName} {{hash: {addressFromHash}}}), " +
                     $"(to:{_addressNodeName} {{hash: {addressToHash}}}) " +
                     $"CREATE (from)-[:{_transitNodeName} {{" +
                     $"clientId: {clientId}, " +
                     $"transitId: {transitId}, " +
                     $"started: datetime(\"{started}\"), " +
                     $"completeAt: datetime(\"{completeAt}\") }}]->(to)");
    await t.CommitAsync();
  }
}