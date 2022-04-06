using LegacyFighter.Cabs.MoneyValue;

namespace LegacyFighter.Cabs.DriverFleet;

public interface IDriverFeeService
{
  Task<Money> CalculateDriverFee(Money transitPrice, long? driverId);
}