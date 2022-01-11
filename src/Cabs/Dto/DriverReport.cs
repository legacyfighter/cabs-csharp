namespace LegacyFighter.Cabs.Dto;

public class DriverReport
{
  public DriverDto DriverDto { get; set; }
  public List<DriverAttributeDto> Attributes { get; set; } = new();
  public IDictionary<DriverSessionDto, List<TransitDto>> Sessions { get; set; } 
    = new Dictionary<DriverSessionDto, List<TransitDto>>();
}