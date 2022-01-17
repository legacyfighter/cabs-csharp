using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Entity.Miles;
using LegacyFighter.Cabs.Repository;
using NodaTime;

namespace LegacyFighter.Cabs.Service;

public class AwardsServiceImpl : IAwardsService
{
  private readonly IAwardsAccountRepository _accountRepository;
  private readonly IClientRepository _clientRepository;
  private readonly ITransitRepository _transitRepository;
  private readonly IClock _clock;
  private readonly IAppProperties _appProperties;

  public AwardsServiceImpl(
    IAwardsAccountRepository accountRepository,
    IClientRepository clientRepository,
    ITransitRepository transitRepository,
    IClock clock,
    IAppProperties appProperties)
  {
    _accountRepository = accountRepository;
    _clientRepository = clientRepository;
    _transitRepository = transitRepository;
    _clock = clock;
    _appProperties = appProperties;
  }

  public async Task<AwardsAccountDto> FindBy(long? clientId)
  {
    return new AwardsAccountDto(await _accountRepository.FindByClient(await _clientRepository.Find(clientId)));
  }

  public async Task RegisterToProgram(long? clientId)
  {
    var client = await _clientRepository.Find(clientId);

    if (client == null)
    {
      throw new ArgumentException("Client does not exists, id = " + clientId);
    }

    var account = AwardsAccount.NotActiveAccount(client, _clock.GetCurrentInstant());
    await _accountRepository.Save(account);
  }

  public async Task ActivateAccount(long? clientId)
  {
    var account = await _accountRepository.FindByClient(await _clientRepository.Find(clientId));

    if (account == null)
    {
      throw new ArgumentException("Account does not exists, id = " + clientId);
    }

    account.Activate();

    await _accountRepository.Save(account);
  }

  public async Task DeactivateAccount(long? clientId)
  {
    var account = await _accountRepository.FindByClient(await _clientRepository.Find(clientId));

    if (account == null)
    {
      throw new ArgumentException("Account does not exists, id = " + clientId);
    }

    account.Deactivate();

    await _accountRepository.Save(account);
  }

  public async Task<AwardedMiles> RegisterMiles(long? clientId, long? transitId)
  {
    var account = await _accountRepository.FindByClient(await _clientRepository.Find(clientId));
    var transit = await _transitRepository.Find(transitId);
    if (transit == null)
    {
      throw new ArgumentException("transit does not exists, id = " + transitId);
    }

    if (account == null || !account.Active)
    {
      return null;
    }
    else
    {
      var expireAt = _clock.GetCurrentInstant().Plus(Duration.FromDays(_appProperties.MilesExpirationInDays));
      var miles = account.AddExpiringMiles(_appProperties.DefaultMilesBonus, expireAt, transit, _clock.GetCurrentInstant());
      await _accountRepository.Save(account);
      return miles;
    }
  }

  private bool IsSunday()
  {
    return _clock.GetCurrentInstant()
      .InZone(DateTimeZoneProviders.Bcl.GetSystemDefault())
      .LocalDateTime.DayOfWeek == IsoDayOfWeek.Sunday;
  }

  public async Task<AwardedMiles> RegisterNonExpiringMiles(long? clientId, int miles)
  {
    var account = await _accountRepository.FindByClient(await _clientRepository.Find(clientId));

    if (account == null)
    {
      throw new ArgumentException("Account does not exists, id = " + clientId);
    }
    else
    {
      var awardedMiles = account.AddNonExpiringMiles(miles, _clock.GetCurrentInstant());
      await _accountRepository.Save(account);
      return awardedMiles;
    }
  }

  public async Task RemoveMiles(long? clientId, int miles)
  {
    var client = await _clientRepository.Find(clientId);
    var account = await _accountRepository.FindByClient(client);

    if (account == null)
    {
      throw new ArgumentException("Account does not exists, id = " + clientId);
    }
    else
    {
      account.Remove(miles, 
        _clock.GetCurrentInstant(), 
        ChooseStrategy(
          (await _transitRepository.FindByClient(client)).Count, 
          client.Claims.Count,
          client.Type, 
          IsSunday()));
    }
  }

  private static Func<List<AwardedMiles>, List<AwardedMiles>> ChooseStrategy(
    int transitsCounter,
    int claimsCounter,
    Client.Types? type,
    bool isSunday)
  {
    if (claimsCounter >= 3)
    {
      return miles => miles.OrderBy(m1 => m1.ExpirationDate.HasValue)
        .ThenByDescending(m2 => m2.ExpirationDate).ToList();
    }
    else if (type == Client.Types.Vip)
    {
      return miles => miles.OrderBy(m3 => m3.CantExpire).ThenBy(m4 => m4.ExpirationDate).ToList();
    }
    else if (transitsCounter >= 15 && isSunday)
    {
      return miles => miles.OrderBy(m5 => m5.CantExpire).ThenBy(m6 => m6.ExpirationDate).ToList();
    }
    else if (transitsCounter >= 15)
    {
      return miles => miles.OrderBy(m7 => m7.CantExpire).ThenBy(m8 => m8.Date).ToList();
    }
    else
    {
      return miles => miles.OrderBy(m9 => m9.Date).ToList();
    }
  }


  public async Task<int> CalculateBalance(long? clientId)
  {
    var client = await _clientRepository.Find(clientId);
    var account = await _accountRepository.FindByClient(client);
    return account.CalculateBalance(_clock.GetCurrentInstant()).Value;
  }

  public async Task TransferMiles(long? fromClientId, long? toClientId, int miles)
  {
    var fromClient = await _clientRepository.Find(fromClientId);
    var accountFrom = await _accountRepository.FindByClient(fromClient);
    var accountTo = await _accountRepository.FindByClient(await _clientRepository.Find(toClientId));
    if (accountFrom == null)
    {
      throw new ArgumentException("Account does not exists, id = " + fromClientId);
    }

    if (accountTo == null)
    {
      throw new ArgumentException("Account does not exists, id = " + toClientId);
    }

    accountFrom.MoveMilesTo(accountTo, miles, _clock.GetCurrentInstant());

    await _accountRepository.Save(accountFrom);
    await _accountRepository.Save(accountTo);
  }
}