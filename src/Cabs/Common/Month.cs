namespace LegacyFighter.Cabs.Common;

public record Month(int Value)
{
  public static readonly Month January = new(1);
  public static readonly Month February = new(2);
  public static readonly Month March = new(3);
  public static readonly Month April = new(4);
  public static readonly Month May = new(5);
  public static readonly Month June = new(6);
  public static readonly Month July = new(7);
  public static readonly Month August = new(8);
  public static readonly Month September = new(9);
  public static readonly Month October = new(10);
  public static readonly Month November = new(11);
  public static readonly Month December = new(12);

  public static IEnumerable<Month> Values()
  {
    return Enumerable.Range(1, 12).Select(i => new Month(i));
  }
}