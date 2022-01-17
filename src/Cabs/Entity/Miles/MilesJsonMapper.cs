using System.Reflection;
using System.Text.Json;
using NodaTime;

namespace LegacyFighter.Cabs.Entity.Miles;

public static class MilesJsonMapper
{
  public static IMiles Deserialize(string json)
  {
    return JsonSerializer.Deserialize<MilesData>(json).ToMiles();
  }

  public static string Serialize(IMiles value)
  {
    return JsonSerializer.Serialize(new MilesData(value));
  }

  private class MilesData
  {
    private const string ExpiringType = "Expiring";
    private const string TwoStepType = "TwoStep";
    public string Type { get; set; }
    public int? Amount { get; set; }
    public long ExpiresAt { get; set; }
    public long WhenFirstHalfExpires { get; set; }

    public MilesData()
    {
    }

    public MilesData(IMiles miles)
    {
      if (miles is ConstantUntil c)
      {
        Type = ExpiringType;
        Amount = c.GetAmountFor(Instant.MinValue);
        ExpiresAt = c.ExpiresAt().ToUnixTimeTicks();
      }
      else if (miles is TwoStepExpiringMiles m)
      {
        Type = TwoStepType;
        Amount = GetPrivateFieldValue<int?>(m, "_amount");
        ExpiresAt = GetPrivateFieldValue<Instant>(m, "_whenExpires").ToUnixTimeTicks();
        WhenFirstHalfExpires = GetPrivateFieldValue<Instant>(m, "_whenFirstHalfExpires").ToUnixTimeTicks();
      }
      else
      {
        throw new NotSupportedException(miles.GetType().ToString());
      }
    }

    private static T GetPrivateFieldValue<T>(TwoStepExpiringMiles m, string amount)
    {
      return (T)typeof(TwoStepExpiringMiles).GetField(amount, BindingFlags.NonPublic | BindingFlags.Instance)?.GetValue(m);
    }

    public IMiles ToMiles()
    {
      if (Type == ExpiringType)
      {
        return new ConstantUntil(Amount, Instant.FromUnixTimeTicks(ExpiresAt));
      }
      else if(Type == TwoStepType)
      {
        return new TwoStepExpiringMiles(
          Amount, 
          Instant.FromUnixTimeTicks(WhenFirstHalfExpires),
          Instant.FromUnixTimeTicks(ExpiresAt));
      }
      else
      {
        throw new NotSupportedException(Type);
      }
    }
  }
}

