using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.Service;

public class DriverFeeService : IDriverFeeService
{
  private readonly IDriverFeeRepository _driverFeeRepository;

  public DriverFeeService(IDriverFeeRepository driverFeeRepository)
  {
    _driverFeeRepository = driverFeeRepository;
  }

  public async Task<Money> CalculateDriverFee(Money transitPrice, long? driverId)
  {
    var driverFee = await _driverFeeRepository.FindByDriverId(driverId);
    if (driverFee == null)
    {
      throw new ArgumentException($"driver Fees not defined for driver, driver id = {driverId}");
    }

    Money finalFee;
    if (driverFee.FeeType == DriverFee.FeeTypes.Flat)
    {
      finalFee = transitPrice - new Money(driverFee.Amount);
    }
    else
    {
      finalFee = transitPrice.Percentage(driverFee.Amount);
    }

    return new Money(Math.Max(finalFee.IntValue, driverFee.Min == null ? 0 : driverFee.Min.IntValue));
  }
}