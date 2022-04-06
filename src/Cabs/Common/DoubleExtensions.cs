namespace LegacyFighter.Cabs.Common;

public static class DoubleExtensions
{
  public static double ToRadians(this double val)
  {
    return (Math.PI / 180) * val;
  }
}