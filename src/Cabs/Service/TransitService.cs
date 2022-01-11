using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using NodaTime;

namespace LegacyFighter.Cabs.Service;

// If this class will still be here in 2022 I will quit.
public class TransitService : ITransitService
{
  private readonly IDriverRepository _driverRepository;
  private readonly ITransitRepository _transitRepository;
  private readonly IClientRepository _clientRepository;
  private readonly InvoiceGenerator _invoiceGenerator;
  private readonly IDriverNotificationService _notificationService;
  private readonly DistanceCalculator _distanceCalculator;
  private readonly IDriverPositionRepository _driverPositionRepository;
  private readonly IDriverSessionRepository _driverSessionRepository;
  private readonly ICarTypeService _carTypeService;
  private readonly IGeocodingService _geocodingService;
  private readonly AddressRepository _addressRepository;
  private readonly IDriverFeeService _driverFeeService;
  private readonly IClock _clock;
  private readonly IAwardsService _awardsService;

  public TransitService(
    IDriverRepository driverRepository,
    ITransitRepository transitRepository,
    IClientRepository clientRepository,
    InvoiceGenerator invoiceGenerator,
    IDriverNotificationService notificationService,
    DistanceCalculator distanceCalculator,
    IDriverPositionRepository driverPositionRepository,
    IDriverSessionRepository driverSessionRepository,
    ICarTypeService carTypeService,
    IGeocodingService geocodingService,
    AddressRepository addressRepository,
    IDriverFeeService driverFeeService,
    IClock clock,
    IAwardsService awardsService)
  {
    _driverRepository = driverRepository;
    _transitRepository = transitRepository;
    _clientRepository = clientRepository;
    _invoiceGenerator = invoiceGenerator;
    _notificationService = notificationService;
    _distanceCalculator = distanceCalculator;
    _driverPositionRepository = driverPositionRepository;
    _driverSessionRepository = driverSessionRepository;
    _carTypeService = carTypeService;
    _geocodingService = geocodingService;
    _addressRepository = addressRepository;
    _driverFeeService = driverFeeService;
    _clock = clock;
    _awardsService = awardsService;
  }

  public async Task<Transit> CreateTransit(TransitDto transitDto)
  {
    var from = await AddressFromDto(transitDto.From);
    var to = await AddressFromDto(transitDto.To);
    return await CreateTransit(transitDto.ClientDto.Id, from, to, transitDto.CarClass);
  }

  private async Task<Address> AddressFromDto(AddressDto addressDto)
  {
    var address = addressDto.ToAddressEntity();
    return await _addressRepository.Save(address);
  }

  public async Task<Transit> CreateTransit(long? clientId, Address from, Address to, CarType.CarClasses? carClass)
  {
    var client = await _clientRepository.Find(clientId);

    if (client == null)
    {
      throw new ArgumentException("Client does not exist, id = " + clientId);
    }

    var transit = new Transit();

    // TODO FIXME later: add some exceptions handling
    var geoFrom = _geocodingService.GeocodeAddress(from);
    var geoTo = _geocodingService.GeocodeAddress(to);

    transit.Client = client;
    transit.From = @from;
    transit.To = to;
    transit.CarType = carClass;
    transit.Status = Transit.Statuses.Draft;
    transit.DateTime = _clock.GetCurrentInstant();
    transit.KmDistance = Distance.OfKm((float)_distanceCalculator.CalculateByMap(geoFrom[0], geoFrom[1], geoTo[0], geoTo[1]));

    return await _transitRepository.Save(transit);
  }

  public async Task ChangeTransitAddressFrom(long? transitId, Address newAddress)
  {
    newAddress = await _addressRepository.Save(newAddress);
    var transit = await _transitRepository.Find(transitId);

    if (transit == null)
    {
      throw new ArgumentException("Transit does not exist, id = " + transitId);
    }

    // TODO FIXME later: add some exceptions handling
    var geoFromNew = _geocodingService.GeocodeAddress(newAddress);
    var geoFromOld = _geocodingService.GeocodeAddress(transit.From);

    // https://www.geeksforgeeks.org/program-distance-two-points-earth/
    // Using extension method ToRadians which converts from
    // degrees to radians.
    var lon1 = geoFromNew[1].ToRadians();
    var lon2 = geoFromOld[1].ToRadians();
    var lat1 = geoFromNew[0].ToRadians();
    var lat2 = geoFromOld[0].ToRadians();

    // Haversine formula
    var dlon = lon2 - lon1;
    var dlat = lat2 - lat1;
    var a = Math.Pow(Math.Sin(dlat / 2), 2)
            + Math.Cos(lat1) * Math.Cos(lat2)
                             * Math.Pow(Math.Sin(dlon / 2), 2);

    var c = 2 * Math.Asin(Math.Sqrt(a));

    // Radius of earth in kilometers. Use 3956 for miles
    double r = 6371;

    // calculate the result
    var distanceInKMeters = c * r;

    if (!(transit.Status == Transit.Statuses.Draft ||
          transit.Status == Transit.Statuses.WaitingForDriverAssignment) ||
        transit.PickupAddressChangeCounter > 2 ||
        distanceInKMeters > 0.25)
    {
      throw new InvalidOperationException("Address 'from' cannot be changed, id = " + transitId);
    }

    transit.From = newAddress;
    transit.KmDistance = Distance.OfKm((float)_distanceCalculator.CalculateByMap(geoFromNew[0], geoFromNew[1], geoFromOld[0], geoFromOld[1]));
    transit.PickupAddressChangeCounter = transit.PickupAddressChangeCounter + 1;
    await _transitRepository.Save(transit);

    foreach (var driver in transit.ProposedDrivers) 
    {
      _notificationService.NotifyAboutChangedTransitAddress(driver.Id, transitId);
    }
  }

  public async Task ChangeTransitAddressTo(long? transitId, AddressDto newAddress)
  {
    await ChangeTransitAddressTo(transitId, newAddress.ToAddressEntity());
  }

  public async Task ChangeTransitAddressFrom(long? transitId, AddressDto newAddress)
  {
    await ChangeTransitAddressFrom(transitId, newAddress.ToAddressEntity());
  }

  public async Task ChangeTransitAddressTo(long? transitId, Address newAddress)
  {
    await _addressRepository.Save(newAddress);
    var transit = await _transitRepository.Find(transitId);

    if (transit == null)
    {
      throw new ArgumentException("Transit does not exist, id = " + transitId);
    }

    if (transit.Status == Transit.Statuses.Completed)
    {
      throw new InvalidOperationException("Address 'to' cannot be changed, id = " + transitId);
    }

    // TODO FIXME later: add some exceptions handling
    var geoFrom = _geocodingService.GeocodeAddress(transit.From);
    var geoTo = _geocodingService.GeocodeAddress(newAddress);

    transit.To = newAddress;
    transit.KmDistance = Distance.OfKm((float)_distanceCalculator.CalculateByMap(geoFrom[0], geoFrom[1], geoTo[0], geoTo[1]));

    if (transit.Driver != null)
    {
      _notificationService.NotifyAboutChangedTransitAddress(transit.Driver.Id, transitId);
    }
  }

  public async Task CancelTransit(long? transitId)
  {
    var transit = await _transitRepository.Find(transitId);

    if (transit == null)
    {
      throw new ArgumentException("Transit does not exist, id = " + transitId);
    }

    if (!new HashSet<Transit.Statuses?>
        { Transit.Statuses.Draft, Transit.Statuses.WaitingForDriverAssignment, Transit.Statuses.TransitToPassenger }
      .Contains(transit.Status))
    {
      throw new InvalidOperationException("Transit cannot be cancelled, id = " + transitId);
    }

    if (transit.Driver != null)
    {
      _notificationService.NotifyAboutCancelledTransit(transit.Driver.Id, transitId);
    }

    transit.Status = Transit.Statuses.Cancelled;
    transit.Driver = null;
    transit.KmDistance = Distance.Zero;
    transit.AwaitingDriversResponses = 0;
    await _transitRepository.Save(transit);
  }

  public async Task<Transit> PublishTransit(long? transitId)
  {
    var transit = await _transitRepository.Find(transitId);

    if (transit == null)
    {
      throw new ArgumentException("Transit does not exist, id = " + transitId);
    }

    transit.Status = Transit.Statuses.WaitingForDriverAssignment;
    transit.Published = _clock.GetCurrentInstant();
    await _transitRepository.Save(transit);

    return await FindDriversForTransit(transitId);
  }

  // Abandon hope all ye who enter here...
  public async Task<Transit> FindDriversForTransit(long? transitId)
  {
    var transit = await _transitRepository.Find(transitId);

    if (transit != null)
    {
      if (transit.Status == Transit.Statuses.WaitingForDriverAssignment)
      {



        var distanceToCheck = 0;

        // Tested on production, works as expected.
        // If you change this code and the system will collapse AGAIN, I'll find you...
        while (true)
        {
          if (transit.AwaitingDriversResponses
              > 4)
          {
            return transit;
          }

          distanceToCheck++;

          // TODO FIXME: to refactor when the final business logic will be determined
          if (transit.Published.Value.Plus(Duration.FromSeconds(300)) < _clock.GetCurrentInstant()
              ||
              (distanceToCheck >= 20)
              ||
              // Should it be here? How is it even possible due to previous status check above loop?
              (transit.Status == Transit.Statuses.Cancelled)
          )
          {
            transit.Status = Transit.Statuses.DriverAssignmentFailed;
            transit.Driver = null;
            transit.KmDistance = Distance.Zero;
            transit.AwaitingDriversResponses = 0;
            await _transitRepository.Save(transit);
            return transit;
          }

          var geocoded = new double[2];


          try
          {
            geocoded = _geocodingService.GeocodeAddress(transit.From);
          }
          catch (Exception e)
          {
            // Geocoding failed! Ask Jessica or Bryan for some help if needed.
          }

          var longitude = geocoded[1];
          var latitude = geocoded[0];

          //https://gis.stackexchange.com/questions/2951/algorithm-for-offsetting-a-latitude-longitude-by-some-amount-of-meters
          //Earth’s radius, sphere
          //double R = 6378;
          double r = 6371; // Changed to 6371 due to Copy&Paste pattern from different source

          //offsets in meters
          double dn = distanceToCheck;
          double de = distanceToCheck;

          //Coordinate offsets in radians
          var dLat = dn / r;
          var dLon = de / (r * Math.Cos(Math.PI * latitude / 180));

          //Offset positions, decimal degrees
          var latitudeMin = latitude - dLat * 180 / Math.PI;
          var latitudeMax = latitude + dLat *
            180 / Math.PI;
          var longitudeMin = longitude - dLon *
            180 / Math.PI;
          var longitudeMax = longitude + dLon * 180 / Math.PI;

          var driversAvgPositions = await _driverPositionRepository
            .FindAverageDriverPositionSince(latitudeMin, latitudeMax, longitudeMin, longitudeMax,
              _clock.GetCurrentInstant().Minus(Duration.FromMinutes(5)));

          if (driversAvgPositions.Any())
          {
            driversAvgPositions.Sort((d1, d2) => 
                Math.Sqrt(Math.Pow(latitude - d1.Latitude, 2) + Math.Pow(longitude - d1.Longitude, 2)).CompareTo(
              Math.Sqrt(Math.Pow(latitude - d2.Latitude, 2) + Math.Pow(longitude - d2.Longitude, 2))
              ));
            driversAvgPositions = driversAvgPositions.Take(20).ToList();

            var carClasses = new List<CarType.CarClasses?>();
            var activeCarClasses = (await _carTypeService.FindActiveCarClasses())
              .Select(c => new CarType.CarClasses?(c)).ToList();
            if (!activeCarClasses.Any())
            {
              return transit;
            }

            if (transit.CarType

                != null)
            {
              if (activeCarClasses.Contains(transit.CarType))
              {
                carClasses.Add(transit.CarType);
              }
              else
              {
                return transit;
              }
            }
            else
            {
              carClasses.AddRange(activeCarClasses);
            }

            var drivers = driversAvgPositions.Select(p => p.Driver).ToList();

            var activeDriverIdsInSpecificCar = (await _driverSessionRepository
              .FindAllByLoggedOutAtNullAndDriverInAndCarClassIn(drivers, carClasses))

              .Select(ds => ds.Driver.Id).ToList();

            driversAvgPositions = driversAvgPositions
              .Where(dp=>activeDriverIdsInSpecificCar.Contains(dp.Driver.Id)).ToList();

            // Iterate across average driver positions
            foreach (var driverAvgPosition in driversAvgPositions) 
            {
              var driver = driverAvgPosition.Driver;
              if (driver.Status == Driver.Statuses.Active &&

                  driver.Occupied == false)
              {
                if (!transit.DriversRejections.Contains(driver))
                {
                  transit.ProposedDrivers.Add(driver);
                  transit.AwaitingDriversResponses = transit.AwaitingDriversResponses + 1;
                  _notificationService.NotifyAboutPossibleTransit(driver.Id, transitId);
                }
              }
              else
              {
                // Not implemented yet!
              }
            }

            await _transitRepository.Save(transit);

          }
          else
          {
            // Next iteration, no drivers at specified area
            continue;
          }
        }
      }
      else
      {
        throw new InvalidOperationException("..., id = " + transitId);
      }
    }
    else
    {
      throw new ArgumentException("Transit does not exist, id = " + transitId);
    }

  }

  public async Task AcceptTransit(long? driverId, long? transitId)
  {
    var driver = await _driverRepository.Find(driverId);

    if (driver == null)
    {
      throw new ArgumentException("Driver does not exist, id = " + driverId);
    }
    else
    {
      var transit = await _transitRepository.Find(transitId);

      if (transit == null)
      {
        throw new ArgumentException("Transit does not exist, id = " + transitId);
      }
      else
      {
        if (transit.Driver != null)
        {
          throw new InvalidOperationException("Transit already accepted, id = " + transitId);
        }
        else
        {
          if (!transit.ProposedDrivers.Contains(driver))
          {
            throw new InvalidOperationException("Driver out of possible drivers, id = " + transitId);
          }
          else
          {
            if (transit.DriversRejections.Contains(driver))
            {
              throw new InvalidOperationException("Driver out of possible drivers, id = " + transitId);
            }
            else
            {
              transit.Driver = driver;
              transit.AwaitingDriversResponses = 0;
              transit.AcceptedAt = _clock.GetCurrentInstant();
              transit.Status = Transit.Statuses.TransitToPassenger;
              await _transitRepository.Save(transit);
              driver.Occupied = true;
              await _driverRepository.Save(driver);
            }
          }
        }
      }
    }
  }

  public async Task StartTransit(long? driverId, long? transitId)
  {
    var driver = _driverRepository.Find(driverId);

    if (driver == null)
    {
      throw new ArgumentException("Driver does not exist, id = " + driverId);
    }

    var transit = await _transitRepository.Find(transitId);

    if (transit == null)
    {
      throw new ArgumentException("Transit does not exist, id = " + transitId);
    }

    if (transit.Status != Transit.Statuses.TransitToPassenger)
    {
      throw new InvalidOperationException("Transit cannot be started, id = " + transitId);
    }

    transit.Status = Transit.Statuses.InTransit;
    transit.Started = SystemClock.Instance.GetCurrentInstant();
    await _transitRepository.Save(transit);
  }

  public async Task RejectTransit(long? driverId, long? transitId)
  {
    var driver = await _driverRepository.Find(driverId);

    if (driver == null)
    {
      throw new ArgumentException("Driver does not exist, id = " + driverId);
    }

    var transit = await _transitRepository.Find(transitId);

    if (transit == null)
    {
      throw new ArgumentException("Transit does not exist, id = " + transitId);
    }

    transit.DriversRejections.Add(driver);
    transit.AwaitingDriversResponses = transit.AwaitingDriversResponses - 1;
    await _transitRepository.Save(transit);
  }

  public async Task CompleteTransit(long? driverId, long? transitId, AddressDto destinationAddress)
  {
    await CompleteTransit(driverId, transitId, destinationAddress.ToAddressEntity());
  }

  public async Task CompleteTransit(long? driverId, long? transitId, Address destinationAddress)
  {
    destinationAddress = await _addressRepository.Save(destinationAddress);
    var driver = await _driverRepository.Find(driverId);

    if (driver == null)
    {
      throw new ArgumentException("Driver does not exist, id = " + driverId);
    }

    var transit = await _transitRepository.Find(transitId);

    if (transit == null)
    {
      throw new ArgumentException("Transit does not exist, id = " + transitId);
    }

    if (transit.Status == Transit.Statuses.InTransit)
    {
      // TODO FIXME later: add some exceptions handling
      var geoFrom = _geocodingService.GeocodeAddress(transit.From);
      var geoTo = _geocodingService.GeocodeAddress(transit.To);

      transit.To = destinationAddress;
      transit.KmDistance = Distance.OfKm((float)_distanceCalculator.CalculateByMap(geoFrom[0], geoFrom[1], geoTo[0], geoTo[1]));
      transit.Status = Transit.Statuses.Completed;
      transit.CalculateFinalCosts();
      driver.Occupied = false;
      transit.CompleteTransitAt(_clock.GetCurrentInstant());
      var driverFee = await _driverFeeService.CalculateDriverFee(transitId);
      transit.DriversFee = driverFee;
      await _driverRepository.Save(driver);
      await _awardsService.RegisterMiles(transit.Client.Id, transitId);
      await _transitRepository.Save(transit);
      await _invoiceGenerator.Generate(transit.Price.IntValue,
        transit.Client.Name + " " + transit.Client.LastName);
    }
    else
    {
      throw new ArgumentException("Cannot complete Transit, id = " + transitId);
    }
  }

  public async Task<TransitDto> LoadTransit(long? id)
  {
    return new TransitDto(await _transitRepository.Find(id));
  }
}