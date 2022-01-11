using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Dto;

public class DriverAttributeDto : IEquatable<DriverAttributeDto>
{
  public DriverAttributeDto(DriverAttribute driverAttribute)
  {
    Name = driverAttribute.Name;
    Value = driverAttribute.Value;
  }

  public DriverAttributeDto(DriverAttribute.DriverAttributeNames name, string value)
  {
    Name = name;
    Value = value;
  }

  private DriverAttributeDto()
  {

  }

  public DriverAttribute.DriverAttributeNames Name { get; set; }
  public string Value { get; set; }

  public bool Equals(DriverAttributeDto other)
  {
    if (ReferenceEquals(null, other)) return false;
    if (ReferenceEquals(this, other)) return true;
    return Name == other.Name && Value == other.Value;
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != this.GetType()) return false;
    return Equals((DriverAttributeDto)obj);
  }

  public override int GetHashCode()
  {
    return HashCode.Combine((int)Name, Value);
  }

  public static bool operator ==(DriverAttributeDto left, DriverAttributeDto right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(DriverAttributeDto left, DriverAttributeDto right)
  {
    return !Equals(left, right);
  }
}