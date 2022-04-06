using LegacyFighter.Cabs.Parties.Api;
using LegacyFighter.Cabs.Parties.Infra;
using LegacyFighter.Cabs.Parties.Model.Parties;

namespace LegacyFighter.Cabs.Parties;

public static class PartiesDependencies
{
  public static void AddTo(WebApplicationBuilder builder)
  {
    builder.Services.AddTransient<PartyMapper>();
    builder.Services.AddTransient<IPartyRepository, EfCorePartyRepository>();
    builder.Services.AddTransient<IPartyRelationshipRepository, EfCorePartyRelationshipRepository>();
  }
}