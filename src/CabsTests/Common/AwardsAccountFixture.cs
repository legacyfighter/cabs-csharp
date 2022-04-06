using LegacyFighter.Cabs.Crm;
using LegacyFighter.Cabs.Loyalty;

namespace LegacyFighter.CabsTests.Common;

public class AwardsAccountFixture
{
  private readonly IAwardsService _awardsService;

  public AwardsAccountFixture(IAwardsService awardsService)
  {
    _awardsService = awardsService;
  }

  public async Task AwardsAccount(Client client) 
  {
    await _awardsService.RegisterToProgram(client.Id);
  }

  public async Task ActiveAwardsAccount(Client client)
  {
    await AwardsAccount(client);
    await _awardsService.ActivateAccount(client.Id);
  }
}