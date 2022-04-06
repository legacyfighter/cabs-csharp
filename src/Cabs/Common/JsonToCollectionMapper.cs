using System.Text.Json;

namespace LegacyFighter.Cabs.Common;

public static class JsonToCollectionMapper
{
  public static ISet<long?> Deserialize(string json)
  {
    if (json == null)
    {
      return new HashSet<long?>();
    }

    return JsonSerializer.Deserialize<ISet<long?>>(json);
  }

  public static string Serialize(ISet<long?> transitsIds)
  {
    return JsonSerializer.Serialize(transitsIds);
  }
}