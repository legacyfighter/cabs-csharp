using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Entity.Events;
using LegacyFighter.Cabs.Invoicing;
using LegacyFighter.Cabs.Notification;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.TransitDetail;
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
  private readonly IAddressRepository _addressRepository;
  private readonly IDriverFeeService _driverFeeService;
  private readonly IClock _clock;
  private readonly IAwardsService _awardsService;
  private readonly EventsPublisher _eventsPublisher;
  private readonly ITransitDetailsFacade _transitDetailsFacade;

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
    IAddressRepository addressRepository,
    IDriverFeeService driverFeeService,
    IClock clock,
    IAwardsService awardsService,
    EventsPublisher eventsPublisher, 
    ITransitDetailsFacade transitDetailsFacade)
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
    _eventsPublisher = eventsPublisher;
    _transitDetailsFacade = transitDetailsFacade;
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

  public async Task<Transit> CreateTransit(long? clientId, Address from, Address to, CarClasses? carClass)
  {
    var client = await _clientRepository.Find(clientId);

    if (client == null)
    {
      throw new ArgumentException("Client does not exist, id = " + clientId);
    }

    // TODO FIXME later: add some exceptions handling
    var geoFrom = _geocodingService.GeocodeAddress(from);
    var geoTo = _geocodingService.GeocodeAddress(to);
    var km = Distance.OfKm((float) _distanceCalculator.CalculateByMap(geoFrom[0], geoFrom[1], geoTo[0], geoTo[1]));
    var now = _clock.GetCurrentInstant();
    var transit = new Transit(now, km);
    var estimatedPrice = transit.EstimateCost();
    transit = await _transitRepository.Save(transit);
    await _transitDetailsFacade.TransitRequested(now, transit.Id, from, to, km, client, carClass, estimatedPrice, transit.Tariff);
    return transit;
  }

  public async Task ChangeTransitAddressFrom(long? transitId, Address newAddress)
  {
    newAddress = await _addressRepository.Save(newAddress);
    var transit = await _transitRepository.Find(transitId);
    var transitDetails = await FindTransitDetails(transitId);

    if (transit == null)
    {
      throw new ArgumentException("Transit does not exist, id = " + transitId);
    }

    // TODO FIXME later: add some exceptions handling
    var geoFromNew = _geocodingService.GeocodeAddress(newAddress);
    var geoFromOld = _geocodingService.GeocodeAddress(transitDetails.From.ToAddressEntity());

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

    var newDistance = Distance.OfKm((float) _distanceCalculator.CalculateByMap(geoFromNew[0], geoFromNew[1], geoFromOld[0], geoFromOld[1]));
    transit.ChangePickupTo(newAddress, newDistance, distanceInKMeters);
    await _transitRepository.Save(transit);
    await _transitDetailsFacade.PickupChangedTo(transit.Id, newAddress, newDistance);

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
    var transitDetails = await FindTransitDetails(transitId);

    if (transit == null)
    {
      throw new ArgumentException("Transit does not exist, id = " + transitId);
    }

    // TODO FIXME later: add some exceptions handling
    var geoFrom = _geocodingService.GeocodeAddress(transitDetails.From.ToAddressEntity());
    var geoTo = _geocodingService.GeocodeAddress(newAddress);

    var newDistance = Distance.OfKm((float) _distanceCalculator.CalculateByMap(geoFrom[0], geoFrom[1], geoTo[0], geoTo[1]));
    transit.ChangeDestinationTo(newAddress, newDistance);
    await _transitDetailsFacade.DestinationChanged(transit.Id, newAddress, newDistance);

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

    if (transit.Driver != null)
    {
      _notificationService.NotifyAboutCancelledTransit(transit.Driver.Id, transitId);
    }

    transit.Cancel();
    await _transitDetailsFacade.TransitCancelled(transitId);
    await _transitRepository.Save(transit);
  }

  public async Task<Transit> PublishTransit(long? transitId)
  {
    var transit = await _transitRepository.Find(transitId);

    if (transit == null)
    {
      throw new ArgumentException("Transit does not exist, id = " + transitId);
    }

    var now = _clock.GetCurrentInstant();
    transit.PublishAt(now);
    await _transitRepository.Save(transit);
    await _transitDetailsFacade.TransitPublished(transitId, now);
    return await FindDriversForTransit(transitId);
  }

  // Abandon hope all ye who enter here...
  public async Task<Transit> FindDriversForTransit(long? transitId)
  {
    var transit = await _transitRepository.Find(transitId);
    var transitDetails = await FindTransitDetails(transitId);

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
          if (transit.ShouldNotWaitForDriverAnyMore(_clock.GetCurrentInstant()) || distanceToCheck >= 20)
          {
            transit.FailDriverAssignment();
            await _transitRepository.Save(transit);
            return transit;
          }

          var geocoded = new double[2];


          try
          {
            geocoded = _geocodingService.GeocodeAddress(await _addressRepository.GetByHash(transitDetails.From.Hash));
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

            var carClasses = new List<CarClasses?>();
            var activeCarClasses = (await _carTypeService.FindActiveCarClasses())
              .Select(c => new CarClasses?(c)).ToList();
            if (!activeCarClasses.Any())
            {
              return transit;
            }

            if (transitDetails.CarType

                != null)
            {
              if (activeCarClasses.Contains(transitDetails.CarType))
              {
                carClasses.Add(transitDetails.CarType);
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

            var drivers = driversAvgPositions.Select(p => p.Driver.Id).ToList();

            var activeDriverIdsInSpecificCar = (await _driverSessionRepository
              .FindAllByLoggedOutAtNullAndDriverIdInAndCarClassIn(drivers, carClasses))

              .Select(ds => ds.DriverId).ToList();

            driversAvgPositions = driversAvgPositions
              .Where(dp=>activeDriverIdsInSpecificCar.Contains(dp.Driver.Id)).ToList();

            // Iterate across average driver positions
            foreach (var driverAvgPosition in driversAvgPositions) 
            {
              var driver = driverAvgPosition.Driver;
              if (driver.Status == Driver.Statuses.Active &&
                  driver.Occupied == false)
              {
                if (transit.CanProposeTo(driver))
                {
                  transit.ProposeTo(driver);
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
        var now = _clock.GetCurrentInstant();
        transit.AcceptBy(driver, now);
        await _transitDetailsFacade.TransitAccepted(transitId, now, driverId);
        await _transitRepository.Save(transit);
        await _driverRepository.Save(driver);
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

    var now = _clock.GetCurrentInstant();
    transit.Start(now);
    await _transitDetailsFacade.TransitStarted(transitId, now);
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

    transit.RejectBy(driver);
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
    var transitDetails = await FindTransitDetails(transitId);

    if (driver == null)
    {
      throw new ArgumentException("Driver does not exist, id = " + driverId);
    }

    var transit = await _transitRepository.Find(transitId);

    if (transit == null)
    {
      throw new ArgumentException("Transit does not exist, id = " + transitId);
    }

    // TODO FIXME later: add some exceptions handling
    var geoFrom = _geocodingService.GeocodeAddress(await _addressRepository.GetByHash(transitDetails.From.Hash));
    var geoTo = _geocodingService.GeocodeAddress(await _addressRepository.GetByHash(transitDetails.To.Hash));
    var distance = Distance.OfKm((float) _distanceCalculator.CalculateByMap(geoFrom[0], geoFrom[1], geoTo[0], geoTo[1]));
    var now = _clock.GetCurrentInstant();
    transit.CompleteTransitAt(now, destinationAddress, distance);
    var driverFee = await _driverFeeService.CalculateDriverFee(transit.Price, driverId);
    driver.Occupied = false;
    await _driverRepository.Save(driver);
    await _awardsService.RegisterMiles(transitDetails.Client.Id, transitId);
    await _transitRepository.Save(transit);
    await _transitDetailsFacade.TransitCompleted(transitId, now, transit.Price, driverFee);
    await _invoiceGenerator.Generate(transit.Price.IntValue, transitDetails.Client.Name + " " + transitDetails.Client.LastName);
    await _eventsPublisher.Publish(
      new TransitCompleted(
        transitDetails.Client.Id,
        transitId,
        transitDetails.From.Hash,
        transitDetails.To.Hash,
        transitDetails.Started!.Value, 
        now,
        _clock.GetCurrentInstant()));
  }

  public async Task<TransitDto> LoadTransit(long? id)
  {
    var transitDetails = await FindTransitDetails(id);
    return new TransitDto(await _transitRepository.Find(id), transitDetails);
  }

  private async Task<TransitDetailsDto> FindTransitDetails(long? transitId) 
  {
    return await _transitDetailsFacade.Find(transitId);
  }
}