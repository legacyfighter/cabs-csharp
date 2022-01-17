using LegacyFighter.Cabs.Config;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using NodaTime;

namespace LegacyFighter.Cabs.Service;

public class AwardsServiceImpl : IAwardsService
{
  private readonly IAwardsAccountRepository _accountRepository;
  private readonly IAwardedMilesRepository _milesRepository;
  private readonly IClientRepository _clientRepository;
  private readonly ITransitRepository _transitRepository;
  private readonly IClock _clock;
  private readonly IAppProperties _appProperties;

  public AwardsServiceImpl(
    IAwardsAccountRepository accountRepository,
    IAwardedMilesRepository milesRepository,
    IClientRepository clientRepository,
    ITransitRepository transitRepository,
    IClock clock,
    IAppProperties appProperties)
  {
    _accountRepository = accountRepository;
    _milesRepository = milesRepository;
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

    var account = new AwardsAccount
    {
      Client = client,
      Active = false,
      Date = _clock.GetCurrentInstant()
    };

    await _accountRepository.Save(account);
  }

  public async Task ActivateAccount(long? clientId)
  {
    var account = await _accountRepository.FindByClient(await _clientRepository.Find(clientId));

    if (account == null)
    {
      throw new ArgumentException("Account does not exists, id = " + clientId);
    }

    account.Active = true;

    await _accountRepository.Save(account);
  }

  public async Task DeactivateAccount(long? clientId)
  {
    var account = await _accountRepository.FindByClient(await _clientRepository.Find(clientId));

    if (account == null)
    {
      throw new ArgumentException("Account does not exists, id = " + clientId);
    }

    account.Active = false;

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

    var now = _clock.GetCurrentInstant();
    if (account == null || !account.Active)
    {
      return null;
    }
    else
    {
      var miles = new AwardedMiles
      {
        Transit = transit,
        Date = _clock.GetCurrentInstant(),
        Client = account.Client,
        Miles = _appProperties.DefaultMilesBonus,
        ExpirationDate = now.Plus(Duration.FromDays(_appProperties.MilesExpirationInDays)),
        CantExpire = false
      };
      account.IncreaseTransactions();

      await _milesRepository.Save(miles);
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
      var nonExpiringMiles = new AwardedMiles
      {
        Transit = null,
        Client = account.Client,
        Miles = miles,
        Date = _clock.GetCurrentInstant(),
        CantExpire = true
      };
      account.IncreaseTransactions();
      await _milesRepository.Save(nonExpiringMiles);
      await _accountRepository.Save(account);
      return nonExpiringMiles;
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
      if (await CalculateBalance(clientId) >= miles && account.Active)
      {
        var milesList = await _milesRepository.FindAllByClient(client);
        var transitsCounter = (await _transitRepository.FindByClient(client)).Count;
        if (client.Claims.Count >= 3)
        {
          milesList = milesList.OrderBy(m => m.ExpirationDate.HasValue)
            .ThenByDescending(m => m.ExpirationDate).ToList();
        }
        else if (client.Type == Client.Types.Vip)
        {
          milesList = milesList.OrderBy(m => m.CantExpire).ThenBy(m => m.ExpirationDate).ToList();
        }
        else if (transitsCounter >= 15 && IsSunday())
        {
          milesList = milesList.OrderBy(m => m.CantExpire).ThenBy(m => m.ExpirationDate).ToList();
        }
        else if (transitsCounter >= 15)
        {
          milesList = milesList.OrderBy(m => m.CantExpire).ThenBy(m => m.Date).ToList();
        }
        else
        {
          milesList = milesList.OrderBy(m => m.Date).ToList();
        }

        foreach (var iter in milesList) 
        {
          if (miles <= 0)
          {
            break;
          }

          if (iter.CantExpire || iter.ExpirationDate > _clock.GetCurrentInstant())
          {
            if (iter.Miles <= miles)
            {
              miles -= iter.Miles;
              iter.Miles = 0;
            }
            else
            {
              iter.Miles = iter.Miles - miles;
              miles = 0;
            }

            await _milesRepository.Save(iter);
          }
        }
      }
      else
      {
        throw new ArgumentException("Insufficient miles, id = " + clientId + ", miles requested = " + miles);
      }
    }

  }

  public async Task<int> CalculateBalance(long? clientId)
  {
    var client = await _clientRepository.Find(clientId);
    var milesList = await _milesRepository.FindAllByClient(client);

    var sum = milesList.Where(t => 
        t.ExpirationDate != null && 
        t.ExpirationDate > _clock.GetCurrentInstant() || 
        t.CantExpire)
      .Select(t => t.Miles).Sum();

    return sum;
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

    if (await CalculateBalance(fromClientId) >= miles && accountFrom.Active)
    {
      var milesList = await _milesRepository.FindAllByClient(fromClient);

      foreach(var iter in milesList) 
      {
        if (iter.CantExpire || iter.ExpirationDate > _clock.GetCurrentInstant())
        {
          if (iter.Miles <= miles)
          {
            iter.Client = accountTo.Client;
            miles -= iter.Miles;
          }
          else
          {
            iter.Miles = iter.Miles - miles;
            var awardedMiles = new AwardedMiles
            {
              Client = accountTo.Client,
              CantExpire = iter.CantExpire,
              ExpirationDate = iter.ExpirationDate,
              Miles = miles
            };

            miles -= iter.Miles;

            await _milesRepository.Save(awardedMiles);

          }

          await _milesRepository.Save(iter);
        }
      }

      accountFrom.IncreaseTransactions();
      accountTo.IncreaseTransactions();

      await _accountRepository.Save(accountFrom);
      await _accountRepository.Save(accountTo);
    }
  }
}