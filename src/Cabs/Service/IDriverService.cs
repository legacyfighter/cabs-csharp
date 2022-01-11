using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.MoneyValue;

namespace LegacyFighter.Cabs.Service;

public interface IDriverService
{
  Task<Driver> CreateDriver(string license, string lastName, string firstName, Driver.Types type,
    Driver.Statuses status, string photo);

  Task ChangeLicenseNumber(string newLicense, long? driverId);
  Task ChangeDriverStatus(long? driverId, Driver.Statuses status);
  Task ChangePhoto(long driverId, string photo);
  Task<Money> CalculateDriverMonthlyPayment(long? driverId, int year, int month);
  Task<Dictionary<Month, Money>> CalculateDriverYearlyPayment(long? driverId, int year);
  Task<DriverDto> LoadDriver(long? driverId);
  Task AddAttribute(long driverId, DriverAttribute.DriverAttributeNames attr, string value);
}