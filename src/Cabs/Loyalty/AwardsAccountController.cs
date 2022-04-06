using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.Loyalty;

[ApiController]
[Route("[controller]")]
public class AwardsAccountController
{
  private readonly IAwardsService _awardsService;

  public AwardsAccountController(IAwardsService awardsService)
  {
    _awardsService = awardsService;
  }

  [HttpPost("/clients/{clientId}/awards")]
  public async Task<IActionResult> Register(long? clientId)
  {
    await _awardsService.RegisterToProgram(clientId);
    return new OkObjectResult(await _awardsService.FindBy(clientId));
  }

  [HttpPost("/clients/{clientId}/awards/activate")]
  public async Task<AwardsAccountDto> Activate(long? clientId)
  {
    await _awardsService.ActivateAccount(clientId);
    return await _awardsService.FindBy(clientId);
  }

  [HttpPost("/clients/{clientId}/awards/deactivate")]
  public async Task<AwardsAccountDto> Deactivate(long? clientId)
  {
    await _awardsService.DeactivateAccount(clientId);
    return await _awardsService.FindBy(clientId);
  }

  [HttpGet("/clients/{clientId}/awards/balance")]
  public async Task<int> CalculateBalance(long? clientId)
  {
    return await _awardsService.CalculateBalance(clientId);
  }

  [HttpPost("/clients/{clientId}/awards/transfer/{toClientId}/{howMuch}")]
  public async Task<AwardsAccountDto> TransferMiles(long? clientId, long? toClientId, int howMuch)
  {
    await _awardsService.TransferMiles(clientId, toClientId, howMuch);
    return await _awardsService.FindBy(clientId);
  }

  [HttpGet("/clients/{clientId}/awards/")]
  public async Task<AwardsAccountDto> FindBy(long? clientId)
  {
    return await _awardsService.FindBy(clientId);
  }
}