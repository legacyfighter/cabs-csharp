namespace LegacyFighter.Cabs.DriverFleet;

public class DriverAttribute
{
  private long Id { get; set; }

  protected DriverAttribute()
  {

  }

  public DriverAttribute(Driver driver, DriverAttributeNames attr, string value)
  {
    Driver = driver;
    Value = value;
    Name = attr;
  }

  internal DriverAttributeNames Name { get; set; }
  internal string Value { get; set; }
  internal virtual Driver Driver { get; set; }
}