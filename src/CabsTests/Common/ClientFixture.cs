using LegacyFighter.Cabs.Crm;

namespace LegacyFighter.CabsTests.Common;

public class ClientFixture
{
  private readonly IClientRepository _clientRepository;

  public ClientFixture(IClientRepository clientRepository)
  {
    _clientRepository = clientRepository;
  }

  public async Task<Client> AClient()
  {
    return await _clientRepository.Save(new Client());
  }

  public async Task<Client> AClient(Client.Types type) 
  {
    var client = new Client
    {
      Type = type
    };
    return await _clientRepository.Save(client);
  }
}