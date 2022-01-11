using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.Service;

public class ClientService : IClientService
{
  private readonly IClientRepository _clientRepository;

  public ClientService(IClientRepository clientRepository)
  {
    _clientRepository = clientRepository;
  }

  public async Task<Client> RegisterClient(string name, string lastName, Client.Types? type, Client.PaymentTypes? paymentType)
  {
    var client = new Client();
    client.Name = name;
    client.LastName = lastName;
    client.Type = type;
    client.DefaultPaymentType = paymentType;
    return await _clientRepository.Save(client);
  }

  public async Task ChangeDefaultPaymentType(long? clientId, Client.PaymentTypes? paymentType)
  {
    var client = await _clientRepository.Find(clientId);
    if (client == null)
    {
      throw new ArgumentException("Client does not exists, id = " + clientId);
    }

    client.DefaultPaymentType = paymentType;
    await _clientRepository.Save(client);
  }

  public async Task UpgradeToVip(long? clientId)
  {
    var client = await _clientRepository.Find(clientId);
    if (client == null)
    {
      throw new ArgumentException("Client does not exists, id = " + clientId);
    }

    client.Type = Client.Types.Vip;
    await _clientRepository.Save(client);
  }

  public async Task DowngradeToRegular(long? clientId)
  {
    var client = await _clientRepository.Find(clientId);
    if (client == null)
    {
      throw new ArgumentException("Client does not exists, id = " + clientId);
    }

    client.Type = Client.Types.Normal;
    await _clientRepository.Save(client);
  }

  public async Task<ClientDto> Load(long? id)
  {
    return new ClientDto(await _clientRepository.Find(id));
  }
}