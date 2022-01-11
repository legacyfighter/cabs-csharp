using System.Globalization;
using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.MoneyValue;
using NodaTime;

namespace LegacyFighter.Cabs.Entity;

public class Tariff
{
  private const int InitialBaseFee = 8;

  public float KmRate { get; }

  public string Name { get; }

  public int BaseFee { get; }

  public Tariff()
  {
  }

  private Tariff(float kmRate, string name, int baseFee)
  {
    KmRate = kmRate;
    Name = name;
    BaseFee = baseFee;
  }

  public static Tariff OfTime(LocalDateTime time)
  {
    if ((time.Month == 12 && time.Day == 31) ||
        (time.Month == 1 && time.Day == 1 && time.Hour <= 6))
    {
      return new Tariff(3.50f, "Sylwester", InitialBaseFee + 3);
    }
    else
    {
      // piątek i sobota po 17 do 6 następnego dnia
      if ((time.DayOfWeek == IsoDayOfWeek.Friday && time.Hour >= 17) ||
          (time.DayOfWeek == IsoDayOfWeek.Saturday && time.Hour <= 6) ||
          (time.DayOfWeek == IsoDayOfWeek.Saturday && time.Hour >= 17) ||
          (time.DayOfWeek == IsoDayOfWeek.Sunday && time.Hour <= 6))
      {
        return new Tariff(2.5f, "Weekend+", InitialBaseFee + 2);
      }
      else
      {
        // pozostałe godziny weekendu
        if ((time.DayOfWeek == IsoDayOfWeek.Saturday && time.Hour > 6 && time.Hour < 17) ||
            (time.DayOfWeek == IsoDayOfWeek.Sunday && time.Hour > 6))
        {
          return new Tariff(1.5f, "Weekend", InitialBaseFee);
        }
        else
        {
          // tydzień roboczy
          return new Tariff(1.0f, "Standard", InitialBaseFee + 1);
        }
      }
    }
  }

  public Money CalculateCost(Distance distance)
  {
    var pricedecimal = new decimal(distance.ToKmInFloat() * KmRate + BaseFee);
    pricedecimal = decimal.Round(pricedecimal, 2, MidpointRounding.ToPositiveInfinity);
    var finalPrice = int.Parse(pricedecimal.ToString("0.00", CultureInfo.InvariantCulture).Replace(".", ""));
    return new Money(finalPrice);
  }
}