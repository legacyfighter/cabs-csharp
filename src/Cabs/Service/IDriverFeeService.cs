using LegacyFighter.Cabs.MoneyValue;

namespace LegacyFighter.Cabs.Service;

public interface IDriverFeeService
{
  Task<Money> CalculateDriverFee(Money transitPrice, long? driverId);
}