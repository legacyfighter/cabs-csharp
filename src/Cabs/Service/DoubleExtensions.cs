namespace LegacyFighter.Cabs.Service;

public static class DoubleExtensions
{
  public static double ToRadians(this double val)
  {
    return (Math.PI / 180) * val;
  }
}