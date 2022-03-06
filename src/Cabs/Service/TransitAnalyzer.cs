using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using NodaTime;

namespace LegacyFighter.Cabs.Service;

public class TransitAnalyzer : ITransitAnalyzer
{
  private readonly ITransitRepository _transitRepository;
  private readonly IClientRepository _clientRepository;
  private readonly IAddressRepository _addressRepository;

  public TransitAnalyzer(ITransitRepository transitRepository, IClientRepository clientRepository, IAddressRepository addressRepository)
  {
    _transitRepository = transitRepository;
    _clientRepository = clientRepository;
    _addressRepository = addressRepository;
  }

  public async Task<List<Address>> Analyze(long? clientId, long? addressId)
  {
    var client = await _clientRepository.Find(clientId);
    if (client == null)
    {
      throw new ArgumentException("Client does not exists, id = " + clientId);
    }

    var address = await _addressRepository.Find(addressId);
    if (address == null)
    {
      throw new ArgumentException("Address does not exists, id = " + addressId);
    }

    return await Analyze(client, address, null);
  }

  // Brace yourself, deadline is coming... They made me to do it this way.
  // Tested!
  private async Task<List<Address>> Analyze(Client client, Address from, Transit t)
  {
    List<Transit> ts;

    if (t == null)
    {
      ts = await _transitRepository.FindAllByClientAndFromAndStatusOrderByDateTimeDesc(client, from,
        Transit.Statuses.Completed);
    }
    else
    {
      ts = await _transitRepository.FindAllByClientAndFromAndPublishedAfterAndStatusOrderByDateTimeDesc(client, from,
        t.Published, Transit.Statuses.Completed);
      ;
    }

    // Workaround for performance reasons.
    if (ts.Count > 1000 && client.Id == 666)
    {
      // No one will see a difference for this customer ;)
      ts = ts.Take(1000).ToList();
    }

//    if (ts.Count == 0)
//    {
//        return new List<Address>() {t.To};
//    }

    if (t != null)
    {
      ts = ts
        .Where(transit=>t.CompleteAt.Value.Plus(Duration.FromMinutes(15)) > transit.Started)
        // Before 2018-01-01:
        //.Where(transit => transit.CompleteAt.Value.Plus(Duration.FromMinutes(15)) > transit.Published)
        .ToList();
    }

    if (ts.Count == 0)
    {
      return new List<Address> { t.To };
    }

    return (await Task.WhenAll(ts.Select(async transit=> {
      List<Address> result = new();
      result.Add(transit.From);
      result.AddRange(await Analyze(client, transit.To, transit));
      return result;
    })))
    .OrderByDescending(list => list.Count)
      .ToList()
      .FirstOrDefault() ?? new List<Address>();
  }
}