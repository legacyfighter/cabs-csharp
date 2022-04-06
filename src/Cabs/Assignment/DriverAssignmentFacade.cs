using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.Notification;
using LegacyFighter.Cabs.Tracking;
using NodaTime;

namespace LegacyFighter.Cabs.Assignment;

public class DriverAssignmentFacade : IDriverAssignmentFacade
{
  private readonly IDriverAssignmentRepository _driverAssignmentRepository;
  private readonly IClock _clock;
  private readonly ICarTypeService _carTypeService;
  private readonly IDriverTrackingService _driverTrackingService;
  private readonly IDriverNotificationService _driverNotificationService;

  public DriverAssignmentFacade(
    IDriverAssignmentRepository driverAssignmentRepository,
    IClock clock,
    ICarTypeService carTypeService,
    IDriverTrackingService driverTrackingService,
    IDriverNotificationService driverNotificationService)
  {
    _driverAssignmentRepository = driverAssignmentRepository;
    _clock = clock;
    _carTypeService = carTypeService;
    _driverTrackingService = driverTrackingService;
    _driverNotificationService = driverNotificationService;
  }

  public async Task<InvolvedDriversSummary> StartAssigningDrivers(
    Guid transitRequestGuid,
    AddressDto from,
    CarClasses? carClass,
    Instant when)
  {
    await _driverAssignmentRepository.Save(new DriverAssignment(transitRequestGuid, when));
    return await SearchForPossibleDrivers(transitRequestGuid, from, carClass);
  }

  public async Task<InvolvedDriversSummary> SearchForPossibleDrivers(
    Guid transitRequestGuid,
    AddressDto from,
    CarClasses? carClass)
  {
    var driverAssignment = await Find(transitRequestGuid);

    if (driverAssignment != null)
    {
      var distanceToCheck = 0;

      // Tested on production, works as expected.
      // If you change this code and the system will collapse AGAIN, I'll find you...
      while (true)
      {
        if (driverAssignment.AwaitingDriversResponses
            > 4)
        {
          return InvolvedDriversSummary.NoneFound();
        }

        distanceToCheck++;

        // FIXME: to refactor when the final business logic will be determined
        if (driverAssignment.ShouldNotWaitForDriverAnyMore(_clock.GetCurrentInstant()) || distanceToCheck >= 20)
        {
          driverAssignment.FailDriverAssignment();
          await _driverAssignmentRepository.Save(driverAssignment);
          return InvolvedDriversSummary.NoneFound();
        }


        var carClasses = await ChoosePossibleCarClasses(carClass);
        if (carClasses.Count == 0)
        {
          return InvolvedDriversSummary.NoneFound();
        }

        var driversAvgPositions =
          await _driverTrackingService.FindActiveDriversNearby(
            from, 
            Distance.OfKm(distanceToCheck), 
            carClasses);

        if (driversAvgPositions.Count == 0)
        {
          //next iteration
          continue;
        }

        // Iterate across average driver positions
        foreach (var driverAvgPosition in driversAvgPositions)
        {
          if (driverAssignment.CanProposeTo(driverAvgPosition.DriverId))
          {
            driverAssignment.ProposeTo(driverAvgPosition.DriverId);
            _driverNotificationService.NotifyAboutPossibleTransit(driverAvgPosition.DriverId, transitRequestGuid);
          }
        }

        await _driverAssignmentRepository.Save(driverAssignment);
        return LoadInvolvedDrivers(driverAssignment);
      }
    }
    else
    {
      throw new ArgumentException("Transit does not exist, id = " + transitRequestGuid);
    }

  }

  private async Task<List<CarClasses>> ChoosePossibleCarClasses(CarClasses? carClass)
  {
    var carClasses = new List<CarClasses>();
    var activeCarClasses = await _carTypeService.FindActiveCarClasses();
    if (carClass != null)
    {
      if (activeCarClasses.Contains(carClass.Value))
      {
        carClasses.Add(carClass.Value);
      }
    }
    else
    {
      carClasses.AddRange(activeCarClasses);
    }

    return carClasses;
  }

  public async Task<InvolvedDriversSummary> AcceptTransit(Guid transitRequestGuid, long? driverId)
  {
    var driverAssignment = await Find(transitRequestGuid);
    driverAssignment.AcceptBy(driverId);
    return LoadInvolvedDrivers(driverAssignment);
  }

  public async Task<InvolvedDriversSummary> RejectTransit(
    Guid transitRequestGuid, 
    long? driverId)
  {
    var driverAssignment = await Find(transitRequestGuid);
    if (driverAssignment == null)
    {
      throw new ArgumentException("Assignment does not exist, id = " + transitRequestGuid);
    }

    driverAssignment.RejectBy(driverId);
    return LoadInvolvedDrivers(driverAssignment);
  }

  public async Task<bool> IsDriverAssigned(Guid transitRequestGuid)
  {
    return await _driverAssignmentRepository
             .FindByRequestGuidAndStatus(
               transitRequestGuid, 
               AssignmentStatuses.OnTheWay) != null;
  }

  private InvolvedDriversSummary LoadInvolvedDrivers(DriverAssignment driverAssignment)
  {
    return new InvolvedDriversSummary(
      driverAssignment.ProposedDrivers,
      driverAssignment.DriverRejections,
      driverAssignment.AssignedDriver,
      driverAssignment.Status);
  }

  public async Task<InvolvedDriversSummary> LoadInvolvedDrivers(Guid transitRequestGuid)
  {
    var driverAssignment = await Find(transitRequestGuid);
    if (driverAssignment == null)
    {
      return InvolvedDriversSummary.NoneFound();
    }

    return LoadInvolvedDrivers(driverAssignment);
  }

  public async Task Cancel(Guid transitRequestGuid)
  {
    var driverAssignment = await Find(transitRequestGuid);
    if (driverAssignment != null)
    {
      driverAssignment.Cancel();
      NotifyAboutCancelledDestination(driverAssignment, transitRequestGuid);
    }
  }

  private async Task<DriverAssignment> Find(Guid transitRequestGuid)
  {
    return await _driverAssignmentRepository.FindByRequestGuid(transitRequestGuid);
  }

  public async Task NotifyAssignedDriverAboutChangedDestination(Guid transitRequestGuid)
  {
    var driverAssignment = await Find(transitRequestGuid);
    if (driverAssignment != null && driverAssignment.AssignedDriver != null)
    {
      var assignedDriver = driverAssignment.AssignedDriver;
      _driverNotificationService.NotifyAboutChangedTransitAddress(assignedDriver, transitRequestGuid);
      foreach (var driver in driverAssignment.ProposedDrivers)
      {
        _driverNotificationService.NotifyAboutChangedTransitAddress(driver, transitRequestGuid);
      }
    }
  }

  public async Task NotifyProposedDriversAboutChangedDestination(Guid transitRequestGuid)
  {
    var driverAssignment = await Find(transitRequestGuid);
    foreach (var driver in driverAssignment.ProposedDrivers)
    {
      _driverNotificationService.NotifyAboutChangedTransitAddress(driver, transitRequestGuid);
    }
  }

  private void NotifyAboutCancelledDestination(DriverAssignment driverAssignment, Guid transitRequestGuid)
  {
    var assignedDriver = driverAssignment.AssignedDriver;
    if (assignedDriver != null)
    {
      _driverNotificationService.NotifyAboutCancelledTransit(assignedDriver, transitRequestGuid);
    }
  }
}