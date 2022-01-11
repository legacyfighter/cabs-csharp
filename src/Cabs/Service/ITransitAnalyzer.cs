using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public interface ITransitAnalyzer
{
  Task<List<Address>> Analyze(long? clientId, long? addressId);
}