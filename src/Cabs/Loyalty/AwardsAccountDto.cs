using LegacyFighter.Cabs.Crm;
using NodaTime;

namespace LegacyFighter.Cabs.Loyalty;

public class AwardsAccountDto
{
  public AwardsAccountDto()
  {
  }

  public AwardsAccountDto(AwardsAccount account, ClientDto clientDto)
  {
    Active = account.Active;
    Client = clientDto;
    Transactions = account.Transactions;
    Date = account.Date;
  }

  public ClientDto Client { set; get; }

  public Instant Date { set; get; }

  public bool Active { set; get; }

  public int Transactions { get; set; }
}