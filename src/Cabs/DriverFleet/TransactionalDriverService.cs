using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.MoneyValue;

namespace LegacyFighter.Cabs.DriverFleet;

public class TransactionalDriverService : IDriverService
{
  private readonly IDriverService _inner;
  private readonly ITransactions _transactions;

  public TransactionalDriverService(IDriverService inner, ITransactions transactions)
  {
    _inner = inner;
    _transactions = transactions;
  }

  public Task<Driver> CreateDriver(string license, string lastName, string firstName, Driver.Types type, Driver.Statuses status, string photo)
  {
    return _inner.CreateDriver(license, lastName, firstName, type, status, photo);
  }

  public async Task ChangeLicenseNumber(string newLicense, long? driverId)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.ChangeLicenseNumber(newLicense, driverId);
    await tx.Commit();
  }

  public async Task ChangeDriverStatus(long? driverId, Driver.Statuses status)
  {
    await using var tx = await _transactions.BeginTransaction();
    await _inner.ChangeDriverStatus(driverId, status);
    await tx.Commit();
  }

  public Task ChangePhoto(long driverId, string photo)
  {
    return _inner.ChangePhoto(driverId, photo);
  }

  public async Task<Money> CalculateDriverMonthlyPayment(long? driverId, int year, int month)
  {
    return await _inner.CalculateDriverMonthlyPayment(driverId, year, month);
  }

  public async Task<Dictionary<Month, Money>> CalculateDriverYearlyPayment(long? driverId, int year)
  {
    return await _inner.CalculateDriverYearlyPayment(driverId, year);
  }

  public async Task<DriverDto> LoadDriver(long? driverId)
  {
    await using var tx = await _transactions.BeginTransaction();
    var driver = await _inner.LoadDriver(driverId);
    await tx.Commit();
    return driver;
  }

  public async Task AddAttribute(long driverId, DriverAttributeNames attr, string value)
  {
    await _inner.AddAttribute(driverId, attr, value);
  }

  public async Task<ISet<DriverDto>> LoadDrivers(ICollection<long?> ids)
  {
    return await _inner.LoadDrivers(ids);
  }
}