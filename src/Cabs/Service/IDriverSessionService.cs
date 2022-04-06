using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public interface IDriverSessionService
{
  Task<DriverSession> LogIn(long? driverId, string plateNumber, CarClasses? carClass, string carBrand);
  Task LogOut(long sessionId);
  Task LogOutCurrentSession(long? driverId);
  Task<List<DriverSession>> FindByDriver(long? driverId);
}