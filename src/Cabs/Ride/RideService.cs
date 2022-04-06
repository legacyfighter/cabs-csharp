using LegacyFighter.Cabs.Assignment;
using LegacyFighter.Cabs.CarFleet;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.DriverFleet;
using LegacyFighter.Cabs.Geolocation.Address;
using LegacyFighter.Cabs.Invoicing;
using LegacyFighter.Cabs.Loyalty;
using LegacyFighter.Cabs.Ride.Details;
using LegacyFighter.Cabs.Ride.Events;
using NodaTime;

namespace LegacyFighter.Cabs.Ride;

// If this class will still be here in 2022 I will quit.
// 20.01.22 - It's a bit better now.
public class RideService : IRideService
{
  private readonly IClientRepository _clientRepository;
  private readonly InvoiceGenerator _invoiceGenerator;
  private readonly IAddressRepository _addressRepository;
  private readonly IDriverFeeService _driverFeeService;
  private readonly IClock _clock;
  private readonly IAwardsService _awardsService;
  private readonly ITransitDetailsFacade _transitDetailsFacade;
  private readonly EventsPublisher _eventsPublisher;
  private readonly IDriverAssignmentFacade _driverAssignmentFacade;
  private readonly IRequestForTransitRepository _requestForTransitRepository;
  private readonly IRequestTransitService _requestTransitService;
  private readonly IChangePickupService _changePickupService;
  private readonly IChangeDestinationService _changeDestinationService;
  private readonly IDemandService _demandService;
  private readonly ICompleteTransitService _completeTransitService;
  private readonly IStartTransitService _startTransitService;
  private readonly IDriverService _driverService;

  public RideService(
    IClientRepository clientRepository,
    InvoiceGenerator invoiceGenerator,
    IAddressRepository addressRepository,
    IDriverFeeService driverFeeService,
    IClock clock,
    IAwardsService awardsService,
    EventsPublisher eventsPublisher, 
    ITransitDetailsFacade transitDetailsFacade,
    IDriverService driverService, 
    IDriverAssignmentFacade driverAssignmentFacade, 
    IRequestForTransitRepository requestForTransitRepository, 
    IRequestTransitService requestTransitService,
    IChangePickupService changePickupService,
    IChangeDestinationService changeDestinationService,
    IDemandService demandService,
    ICompleteTransitService completeTransitService,
    IStartTransitService startTransitService)
  {
    _clientRepository = clientRepository;
    _invoiceGenerator = invoiceGenerator;
    _addressRepository = addressRepository;
    _driverFeeService = driverFeeService;
    _clock = clock;
    _awardsService = awardsService;
    _eventsPublisher = eventsPublisher;
    _transitDetailsFacade = transitDetailsFacade;
    _driverService = driverService;
    _driverAssignmentFacade = driverAssignmentFacade;
    _requestForTransitRepository = requestForTransitRepository;
    _requestTransitService = requestTransitService;
    _changePickupService = changePickupService;
    _changeDestinationService = changeDestinationService;
    _demandService = demandService;
    _completeTransitService = completeTransitService;
    _startTransitService = startTransitService;
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
    await _transitDetailsFacade.TransitRequested(
      now,
      requestForTransit.RequestGuid,
      from,
      to,
      requestForTransit.Distance,
      client,
      carClass,
      requestForTransit.EstimatedPrice,
      requestForTransit.Tariff);
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

  public async Task ChangeTransitAddressTo(Guid requestGuid, AddressDto newAddress)
  {
    await ChangeTransitAddressTo(requestGuid, newAddress.ToAddressEntity());
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

  public async Task PublishTransit(Guid requestGuid)
  {
    var transitDetailsDto = await _transitDetailsFacade.Find(requestGuid);

    if (transitDetailsDto == null) 
    {
      throw new InvalidOperationException($"Transit does not exist, id = {requestGuid}");
    }

    await _demandService.PublishDemand(requestGuid);
    await _driverAssignmentFacade.StartAssigningDrivers(
      requestGuid,
      transitDetailsDto.From,
      transitDetailsDto.CarType,
      _clock.GetCurrentInstant());
    await _transitDetailsFacade.TransitPublished(requestGuid, _clock.GetCurrentInstant());
  }

  public async Task CancelTransit(Guid requestGuid)
  {
    var transitDetailsDto = await _transitDetailsFacade.Find(requestGuid);
    if (transitDetailsDto == null) {
      throw new ArgumentException($"Transit does not exist, id = {requestGuid}");
    }
    await _demandService.CancelDemand(requestGuid);
    await _driverAssignmentFacade.Cancel(requestGuid);
    await _transitDetailsFacade.TransitCancelled(requestGuid);
  }

  public async Task<TransitDetailsDto> FindDriversForTransit(Guid requestGuid)
  {
    var transitDetailsDto = await _transitDetailsFacade.Find(requestGuid);
    var involvedDriversSummary = await _driverAssignmentFacade.SearchForPossibleDrivers(requestGuid, transitDetailsDto.From, transitDetailsDto.CarType.Value);
    await _transitDetailsFacade.DriversAreInvolved(requestGuid, involvedDriversSummary);
    return await _transitDetailsFacade.Find(requestGuid);
  }

  public async Task AcceptTransit(long? driverId, Guid requestGuid)
  {
    if (!await _driverService.Exists(driverId))
    {
      throw new ArgumentException($"Driver does not exist, id = {driverId}");
    }
    else
    {
      if (await _driverAssignmentFacade.IsDriverAssigned(requestGuid))
      {
        throw new InvalidOperationException($"Driver already assigned, requestGuid = {requestGuid}");
      }

      await _demandService.AcceptDemand(requestGuid);
      await _driverAssignmentFacade.AcceptTransit(requestGuid, driverId);
      await _driverService.MarkOccupied(driverId);
      await _transitDetailsFacade.TransitAccepted(requestGuid, driverId, _clock.GetCurrentInstant());
    }
  }

  public async Task StartTransit(long? driverId, Guid requestGuid)
  {
    if (!await _driverService.Exists(driverId))
    {
      throw new ArgumentException($"Driver does not exist, id = {driverId}");
    }

    if (!await _demandService.ExistsFor(requestGuid))
    {
      throw new ArgumentException($"Transit does not exist, id = {requestGuid}");
    }

    if (!await _driverAssignmentFacade.IsDriverAssigned(requestGuid))
    {
      throw new InvalidOperationException($"Driver not assigned, requestGuid = {requestGuid}");
    }

    var now = _clock.GetCurrentInstant();
    var transit = await _startTransitService.Start(requestGuid);
    await _transitDetailsFacade.TransitStarted(requestGuid, transit.Id, now);
  }

  public async Task RejectTransit(long? driverId, Guid requestGuid)
  {
    if (!await _driverService.Exists(driverId)) 
    {
      throw new ArgumentException($"Driver does not exist, id = {driverId}");
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
    var transitDetails = await _transitDetailsFacade.Find(requestGuid);
    if (!await _driverService.Exists(driverId)) 
    {
      throw new ArgumentException($"Driver does not exist, id = {driverId}");
    }
    var from = await _addressRepository.GetByHash(transitDetails.From.Hash);
    var to = await _addressRepository.GetByHash(destinationAddress.Hash);
    var finalPrice = await _completeTransitService.CompleteTransit(driverId, requestGuid, from, to);
    var driverFee = await _driverFeeService.CalculateDriverFee(finalPrice, driverId);
    await _driverService.MarkNotOccupied(driverId);
    await _transitDetailsFacade.TransitCompleted(requestGuid, _clock.GetCurrentInstant(), finalPrice, driverFee);
    await _awardsService.RegisterMiles(transitDetails.Client.Id, transitDetails.TransitId);
    await _invoiceGenerator.Generate(finalPrice.IntValue,
      $"{transitDetails.Client.Name} {transitDetails.Client.LastName}");
    await _eventsPublisher.Publish(new TransitCompleted(
      transitDetails.Client.Id,
      transitDetails.TransitId,
      transitDetails.From.Hash,
      destinationAddress.Hash,
      transitDetails.Started.Value,
      _clock.GetCurrentInstant(),
      _clock.GetCurrentInstant())
    );
  }

  public async Task<TransitDto> LoadTransit(Guid requestGuid)
  {
    var involvedDriversSummary = await _driverAssignmentFacade.LoadInvolvedDrivers(requestGuid);
    var transitDetails = await _transitDetailsFacade.Find(requestGuid);
    var proposedDrivers = await _driverService.LoadDrivers(involvedDriversSummary.ProposedDrivers);
    var driverRejections = await _driverService.LoadDrivers(involvedDriversSummary.DriverRejections);
    return new TransitDto(
      transitDetails,
      proposedDrivers,
      driverRejections,
      involvedDriversSummary.AssignedDriver);
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