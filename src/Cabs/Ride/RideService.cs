using LegacyFighter.Cabs.Assignment;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.Geolocation;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.Invoicing;
using LegacyFighter.Cabs.Loyalty;
using LegacyFighter.Cabs.Pricing;
using LegacyFighter.Cabs.Ride.Details;
using LegacyFighter.Cabs.Ride.Events;
using NodaTime;

namespace LegacyFighter.Cabs.Ride;

// If this class will still be here in 2022 I will quit.
public class RideService : IRideService
{
  private readonly IDriverRepository _driverRepository;
  private readonly ITransitRepository _transitRepository;
  private readonly IClientRepository _clientRepository;
  private readonly InvoiceGenerator _invoiceGenerator;
  private readonly DistanceCalculator _distanceCalculator;
  private readonly IGeocodingService _geocodingService;
  private readonly IAddressRepository _addressRepository;
  private readonly IDriverFeeService _driverFeeService;
  private readonly IClock _clock;
  private readonly IAwardsService _awardsService;
  private readonly ITransitDetailsFacade _transitDetailsFacade;
  private readonly EventsPublisher _eventsPublisher;
  private readonly IDriverAssignmentFacade _driverAssignmentFacade;
  private readonly IRequestForTransitRepository _requestForTransitRepository;
  private readonly ITransitDemandRepository _transitDemandRepository;
  private readonly IRequestTransitService _requestTransitService;
  private readonly IChangePickupService _changePickupService;
  private readonly IChangeDestinationService _changeDestinationService;
  private readonly Tariffs _tariffs;
  private readonly IDriverService _driverService;

  public RideService(
    IDriverRepository driverRepository,
    ITransitRepository transitRepository,
    IClientRepository clientRepository,
    InvoiceGenerator invoiceGenerator,
    DistanceCalculator distanceCalculator,
    IGeocodingService geocodingService,
    IAddressRepository addressRepository,
    IDriverFeeService driverFeeService,
    IClock clock,
    IAwardsService awardsService,
    EventsPublisher eventsPublisher, 
    ITransitDetailsFacade transitDetailsFacade,
    IDriverService driverService, 
    IDriverAssignmentFacade driverAssignmentFacade, 
    IRequestForTransitRepository requestForTransitRepository, 
    ITransitDemandRepository transitDemandRepository, 
    IRequestTransitService requestTransitService,
    IChangePickupService changePickupService,
    IChangeDestinationService changeDestinationService,
    Tariffs tariffs)
  {
    _driverRepository = driverRepository;
    _transitRepository = transitRepository;
    _clientRepository = clientRepository;
    _invoiceGenerator = invoiceGenerator;
    _distanceCalculator = distanceCalculator;
    _geocodingService = geocodingService;
    _addressRepository = addressRepository;
    _driverFeeService = driverFeeService;
    _clock = clock;
    _awardsService = awardsService;
    _eventsPublisher = eventsPublisher;
    _transitDetailsFacade = transitDetailsFacade;
    _driverService = driverService;
    _driverAssignmentFacade = driverAssignmentFacade;
    _requestForTransitRepository = requestForTransitRepository;
    _transitDemandRepository = transitDemandRepository;
    _requestTransitService = requestTransitService;
    _changePickupService = changePickupService;
    _changeDestinationService = changeDestinationService;
    _tariffs = tariffs;
  }

  public async Task<TransitDto> CreateTransit(TransitDto transitDto)
  {
    return await CreateTransit(
      transitDto.ClientDto.Id,
      transitDto.From,
      transitDto.To,
      transitDto.CarClass);
  }

  public async Task<TransitDto> CreateTransit(
    long? clientId,
    AddressDto fromDto,
    AddressDto toDto,
    CarClasses? carClass)
  {
    var client = await FindClient(clientId);
    var from = await AddressFromDto(fromDto);
    var to = await AddressFromDto(toDto);
    var now = _clock.GetCurrentInstant();
    var requestForTransit = await _requestTransitService.CreateRequestForTransit(from, to);
    await _transitDetailsFacade.TransitRequested(now, requestForTransit.RequestGuid, from, to, requestForTransit.Distance, client, carClass, requestForTransit.EstimatedPrice, requestForTransit.Tariff);
    return await LoadTransit(requestForTransit.Id);
  }

  public async Task ChangeTransitAddressFrom(Guid requestGuid, Address newAddress)
  {
    if (await _driverAssignmentFacade.IsDriverAssigned(requestGuid))
    {
      throw new InvalidOperationException($"Driver already assigned, requestGuid = {requestGuid}");
    }

    newAddress = await _addressRepository.Save(newAddress);
    var transitDetails = await _transitDetailsFacade.Find(requestGuid);
    var oldAddress = transitDetails.From.ToAddressEntity();
    var newDistance = await _changePickupService.ChangeTransitAddressFrom(requestGuid, newAddress, oldAddress);
    await _transitDetailsFacade.PickupChangedTo(requestGuid, newAddress, newDistance);
    await _driverAssignmentFacade.NotifyProposedDriversAboutChangedDestination(requestGuid);
  }

  public async Task ChangeTransitAddressFrom(Guid requestGuid, AddressDto newAddress)
  {
    await ChangeTransitAddressFrom(requestGuid, newAddress.ToAddressEntity());
  }

  private async Task<Client> FindClient(long? clientId)
  {
    var client = await _clientRepository.Find(clientId);
    if (client == null)
    {
      throw new ArgumentException($"Client does not exist, id = {clientId}");
    }
    return client;
  }

  private async Task<Address> AddressFromDto(AddressDto addressDto)
  {
    var address = addressDto.ToAddressEntity();
    return await _addressRepository.Save(address);
  }

  public async Task ChangeTransitAddressTo(Guid requestGuid, AddressDto newAddress)
  {
    await ChangeTransitAddressTo(requestGuid, newAddress.ToAddressEntity());
  }

  public async Task ChangeTransitAddressTo(Guid requestGuid, Address newAddress)
  {
    newAddress = await _addressRepository.Save(newAddress);
    var transitDetails = await _transitDetailsFacade.Find(requestGuid);
    if (transitDetails == null)
    {
      throw new ArgumentException($"Transit does not exist, id = {requestGuid}");
    }
    var oldAddress = transitDetails.From.ToAddressEntity();
    var distance = await _changeDestinationService
      .ChangeTransitAddressTo(requestGuid, newAddress, oldAddress);
    await _driverAssignmentFacade.NotifyAssignedDriverAboutChangedDestination(requestGuid);
    await _transitDetailsFacade.DestinationChanged(requestGuid, newAddress, distance);
  }

  public async Task CancelTransit(Guid requestGuid)
  {
    var requestForTransit = await _requestForTransitRepository.FindByRequestGuid(requestGuid);

    if (requestForTransit == null)
    {
      throw new ArgumentException($"Transit does not exist, id = {requestGuid}");
    }

    var transitDemand = await _transitDemandRepository.FindByTransitRequestGuid(requestGuid);
    if (transitDemand != null) 
    {
      transitDemand.Cancel();
      await _driverAssignmentFacade.Cancel(requestGuid);
    }
    await _transitDetailsFacade.TransitCancelled(requestGuid);
  }

  public async Task<Transit> PublishTransit(Guid requestGuid)
  {
    var requestFor = await _requestForTransitRepository.FindByRequestGuid(requestGuid);
    var transitDetailsDto = await _transitDetailsFacade.Find(requestGuid);

    if (requestFor == null) 
    {
      throw new InvalidOperationException($"Transit does not exist, id = {requestGuid}");
    }

    var now = _clock.GetCurrentInstant();
    await _transitDemandRepository.Save(new TransitDemand(requestFor.RequestGuid));
    await _driverAssignmentFacade.CreateAssignment(requestGuid, transitDetailsDto.From, transitDetailsDto.CarType, now);
    await _transitDetailsFacade.TransitPublished(requestGuid, now);
    return await _transitRepository.FindByTransitRequestGuid(requestGuid);
  }

  // Abandon hope all ye who enter here...
  public async Task<Transit> FindDriversForTransit(Guid requestGuid)
  {
    var transitDetailsDto = await _transitDetailsFacade.Find(requestGuid);
    var involvedDriversSummary = await _driverAssignmentFacade.SearchForPossibleDrivers(requestGuid, transitDetailsDto.From, transitDetailsDto.CarType.Value);
    await _transitDetailsFacade.DriversAreInvolved(requestGuid, involvedDriversSummary);
    return await _transitRepository.FindByTransitRequestGuid(requestGuid);
  }

  public async Task AcceptTransit(long? driverId, Guid requestGuid)
  {
    var driver = await _driverRepository.Find(driverId);
    var transitDemand = await _transitDemandRepository.FindByTransitRequestGuid(requestGuid);

    if (driver == null)
    {
      throw new ArgumentException($"Driver does not exist, id = {driverId}");
    }
    else
    {
      if (await _driverAssignmentFacade.IsDriverAssigned(requestGuid))
      {
        throw new InvalidOperationException($"Driver already assigned, requestGuid = {requestGuid}");
      }

      if (transitDemand == null)
      {
        throw new ArgumentException($"Transit does not exist, id = {requestGuid}");
      }
      else
      {
        var now = _clock.GetCurrentInstant();
        transitDemand.Accepted();
        await _driverAssignmentFacade.AcceptTransit(requestGuid, driver);
        await _transitDetailsFacade.TransitAccepted(requestGuid, driverId, now);
        await _driverRepository.Save(driver);
      }
    }
  }

  public async Task StartTransit(long? driverId, Guid requestGuid)
  {
    var driver = _driverRepository.Find(driverId);

    if (driver == null)
    {
      throw new ArgumentException($"Driver does not exist, id = {driverId}");
    }

    var transitDemand = await _transitDemandRepository.FindByTransitRequestGuid(requestGuid);

    if (transitDemand == null)
    {
      throw new ArgumentException($"Transit does not exist, id = {requestGuid}");
    }

    if (!await _driverAssignmentFacade.IsDriverAssigned(requestGuid))
    {
      throw new InvalidOperationException($"Driver not assigned, requestGuid = {requestGuid}");
    }

    var now = _clock.GetCurrentInstant();
    var transit = new Transit(_tariffs.Choose(now), requestGuid);
    await _transitRepository.Save(transit);
    await _transitDetailsFacade.TransitStarted(requestGuid, transit.Id, now);
  }

  public async Task RejectTransit(long? driverId, Guid requestGuid)
  {
    var driver = await _driverRepository.Find(driverId);

    if (driver == null)
    {
      throw new ArgumentException("Driver does not exist, id = " + driverId);
    }

    await _driverAssignmentFacade.RejectTransit(requestGuid, driverId);
  }

  public async Task CompleteTransit(long? driverId, Guid requestGuid, AddressDto destinationAddress)
  {
    await CompleteTransit(driverId, requestGuid, destinationAddress.ToAddressEntity());
  }

  public async Task CompleteTransit(long? driverId, Guid requestGuid, Address destinationAddress)
  {
    destinationAddress = await _addressRepository.Save(destinationAddress);
    var driver = await _driverRepository.Find(driverId);
    var transitDetails = await _transitDetailsFacade.Find(requestGuid);

    if (driver == null)
    {
      throw new ArgumentException($"Driver does not exist, id = {driverId}");
    }

    var transit = await _transitRepository.FindByTransitRequestGuid(requestGuid);

    if (transit == null)
    {
      throw new ArgumentException($"Transit does not exist, id = {requestGuid}");
    }

    // TODO FIXME later: add some exceptions handling
    var geoFrom = _geocodingService.GeocodeAddress(await _addressRepository.GetByHash(transitDetails.From.Hash));
    var geoTo = _geocodingService.GeocodeAddress(await _addressRepository.GetByHash(transitDetails.To.Hash));
    var distance = Distance.OfKm((float) _distanceCalculator.CalculateByMap(geoFrom[0], geoFrom[1], geoTo[0], geoTo[1]));
    var now = _clock.GetCurrentInstant();
    var finalPrice = transit.CompleteTransitAt(distance);
    var driverFee = await _driverFeeService.CalculateDriverFee(finalPrice, driverId);
    driver.Occupied = false;
    await _driverRepository.Save(driver);
    await _awardsService.RegisterMiles(transitDetails.Client.Id, transit.Id);
    await _transitRepository.Save(transit);
    await _transitDetailsFacade.TransitCompleted(requestGuid, now, finalPrice, driverFee);
    await _invoiceGenerator.Generate(finalPrice.IntValue, transitDetails.Client.Name + " " + transitDetails.Client.LastName);
    await _eventsPublisher.Publish(
      new TransitCompleted(
        transitDetails.Client.Id,
        transit.Id,
        transitDetails.From.Hash,
        transitDetails.To.Hash,
        transitDetails.Started!.Value, 
        now,
        _clock.GetCurrentInstant()));
  }

  public async Task<TransitDto> LoadTransit(Guid requestGuid)
  {
    var involvedDriversSummary = await _driverAssignmentFacade.LoadInvolvedDrivers(requestGuid);
    var transitDetails = await _transitDetailsFacade.Find(requestGuid);
    var proposedDrivers = await _driverService.LoadDrivers(involvedDriversSummary.ProposedDrivers);
    var driverRejections = await _driverService.LoadDrivers(involvedDriversSummary.DriverRejections);
    return new TransitDto(transitDetails, proposedDrivers, driverRejections, involvedDriversSummary.AssignedDriver);
  }

  public async Task<TransitDto> LoadTransit(long? requestId) 
  {
    var requestGuid = await GetRequestGuid(requestId);
    return await LoadTransit(requestGuid);
  }

  public async Task<Guid> GetRequestGuid(long? requestId)
  {
    return (await _requestForTransitRepository.Find(requestId)).RequestGuid;
  }
}