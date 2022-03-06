using Neo4j.Driver;
using NodaTime;

namespace LegacyFighter.Cabs.TransitAnalyzer;

public class GraphTransitAnalyzer
{
  private readonly IDriver _driver;

  public GraphTransitAnalyzer(IDriver driver)
  {
    _driver = driver;
  }

  public async Task<List<long?>> Analyze(long? clientId, int? addressHash)
  {
    await using var session = _driver.AsyncSession();
    await using var t = await session.BeginTransactionAsync();
    var result = await t.RunAsync(
      $"MATCH p=(a:Address)-[:Transit*]->(:Address) WHERE a.hash = {addressHash} " +
      $"AND (ALL(x IN range(1, length(p)-1) WHERE ((relationships(p)[x]).clientId = {clientId}) " +
      "AND 0 <= duration.inSeconds( (relationships(p)[x-1]).completeAt, (relationships(p)[x]).started).minutes <= 15)) " +
      "AND length(p) >= 1 " +
      "RETURN [x in nodes(p) | x.hash] AS hashes " +
      "ORDER BY length(p) DESC LIMIT 1");
    var hashes = ((List<object>)(await result.ToListAsync())[0].Values["hashes"]).Cast<long?>().ToList();
    await t.CommitAsync();
    return hashes;
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
    await t.RunAsync($"MERGE (from:Address {{hash: {addressFromHash}}})");
    await t.RunAsync($"MERGE (to:Address {{hash: {addressToHash}}})");
    await t.RunAsync("MATCH " +
                     $"(from:Address {{hash: {addressFromHash}}}), " +
                     $"(to:Address {{hash: {addressToHash}}}) " +
                     "CREATE (from)-[:Transit {" +
                     $"clientId: {clientId}, " +
                     $"transitId: {transitId}, " +
                     $"started: datetime(\"{started}\"), " +
                     $"completeAt: datetime(\"{completeAt}\") }}]->(to)");
    await t.CommitAsync();
  }
}