using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Service;
using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.Controllers;

[ApiController]
[Route("[controller]")]
public class ClientController
{
  internal IClientService ClientService;

  public ClientController(IClientService clientService)
  {
    ClientService = clientService;
  }

  [HttpPost("/clients")]
  public async Task<ClientDto> Register([FromBody] ClientDto dto)
  {
    var c = await ClientService.RegisterClient(dto.Name, dto.LastName, dto.Type,
      dto.DefaultPaymentType);
    return await ClientService.Load(c.Id);
  }

  [HttpGet("/clients/{clientId}")]
  public async Task<ClientDto> Find(long? clientId)
  {
    return await ClientService.Load(clientId);
  }

  [HttpPost("/clients/{clientId}/upgrade")]
  public async Task<ClientDto> UpgradeToVip(long? clientId)
  {
    await ClientService.UpgradeToVip(clientId);
    return await ClientService.Load(clientId);
  }

  [HttpPost("/clients/{clientId}/downgrade")]
  public async Task<ClientDto> Downgrade(long? clientId)
  {
    await ClientService.DowngradeToRegular(clientId);
    return await ClientService.Load(clientId);
  }

  [HttpPost("/clients/{clientId}/changeDefaultPaymentType")]
  public async Task<ClientDto> ChangeDefaultPaymentType(long? clientId, [FromBody] ClientDto dto)
  {
    await ClientService.ChangeDefaultPaymentType(clientId, dto.DefaultPaymentType);
    return await ClientService.Load(clientId);
  }
}