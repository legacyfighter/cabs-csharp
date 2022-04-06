using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using NodaTime;

namespace LegacyFighter.Cabs.Service;

public class DriverSessionService : IDriverSessionService
{
  private readonly IDriverSessionRepository _driverSessionRepository;
  private readonly ICarTypeService _carTypeService;
  private readonly IClock _clock;

  public DriverSessionService(IDriverSessionRepository driverSessionRepository, ICarTypeService carTypeService, IClock clock)
  {
    _driverSessionRepository = driverSessionRepository;
    _carTypeService = carTypeService;
    _clock = clock;
  }

  public async Task<DriverSession> LogIn(long? driverId, string plateNumber, CarClasses? carClass, string carBrand)
  {
    var session = new DriverSession
    {
      DriverId = driverId,
      LoggedAt = _clock.GetCurrentInstant(),
      CarClass = carClass,
      PlatesNumber = plateNumber,
      CarBrand = carBrand
    };
    await _carTypeService.RegisterActiveCar(session.CarClass);
    return await _driverSessionRepository.Save(session);
  }

  public async Task LogOut(long sessionId)
  {
    var session = await _driverSessionRepository.Find(sessionId);
    if (session == null)
    {
      throw new ArgumentException("Session does not exist");
    }

    await _carTypeService.UnregisterCar(session.CarClass);
    session.LoggedOutAt = _clock.GetCurrentInstant();
  }

  public async Task LogOutCurrentSession(long? driverId)
  {
    var session =
      await _driverSessionRepository
        .FindTopByDriverIdAndLoggedOutAtIsNullOrderByLoggedAtDesc(driverId);
    if (session != null)
    {
      session.LoggedOutAt = _clock.GetCurrentInstant();
      await _carTypeService.UnregisterCar(session.CarClass);
    }
  }

  public async Task<List<DriverSession>> FindByDriver(long? driverId)
  {
    return await _driverSessionRepository.FindByDriverId(driverId);
  }
}