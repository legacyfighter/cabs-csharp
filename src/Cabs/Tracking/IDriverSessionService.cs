using LegacyFighter.Cabs.CarFleet;

namespace LegacyFighter.Cabs.Tracking;

public interface IDriverSessionService
{
  Task<DriverSession> LogIn(long? driverId, string plateNumber, CarClasses? carClass, string carBrand);
  Task LogOut(long sessionId);
  Task LogOutCurrentSession(long? driverId);
  Task<List<DriverSession>> FindByDriver(long? driverId);
  Task<List<long?>> FindCurrentlyLoggedDriverIds(List<long?> driversIds, List<CarClasses> carClasses);
}