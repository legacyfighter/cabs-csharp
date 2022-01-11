namespace LegacyFighter.Cabs.Service;

public interface IDriverFeeService
{
  Task<int> CalculateDriverFee(long? transitId);
}