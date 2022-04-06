using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.Crm;

[ApiController]
[Route("[controller]")]
public class ClientController
{
  private readonly IClientService _clientService;

  public ClientController(IClientService clientService)
  {
    _clientService = clientService;
  }

  [HttpPost("/clients")]
  public async Task<ClientDto> Register([FromBody] ClientDto dto)
  {
    var c = await _clientService.RegisterClient(dto.Name, dto.LastName, dto.Type,
      dto.DefaultPaymentType);
    return await _clientService.Load(c.Id);
  }

  [HttpGet("/clients/{clientId}")]
  public async Task<ClientDto> Find(long? clientId)
  {
    return await _clientService.Load(clientId);
  }

  [HttpPost("/clients/{clientId}/upgrade")]
  public async Task<ClientDto> UpgradeToVip(long? clientId)
  {
    await _clientService.UpgradeToVip(clientId);
    return await _clientService.Load(clientId);
  }

  [HttpPost("/clients/{clientId}/downgrade")]
  public async Task<ClientDto> Downgrade(long? clientId)
  {
    await _clientService.DowngradeToRegular(clientId);
    return await _clientService.Load(clientId);
  }

  [HttpPost("/clients/{clientId}/changeDefaultPaymentType")]
  public async Task<ClientDto> ChangeDefaultPaymentType(long? clientId, [FromBody] ClientDto dto)
  {
    await _clientService.ChangeDefaultPaymentType(clientId, dto.DefaultPaymentType);
    return await _clientService.Load(clientId);
  }
}