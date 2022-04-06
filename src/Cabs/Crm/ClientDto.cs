namespace LegacyFighter.Cabs.Crm;

public class ClientDto
{
  public ClientDto()
  {
  }

  public ClientDto(Client client)
  {
    Id = client.Id;
    Type = client.Type;
    Name = client.Name;
    LastName = client.LastName;
    DefaultPaymentType = client.DefaultPaymentType;
    ClientType = client.ClientType;
  }

  private int? NumberOfClaims { get; set; }
  public string Name { get; set; }
  public string LastName { get; set; }
  public Client.ClientTypes? ClientType { get; set; }
  public Client.Types? Type { get; set; }
  public Client.PaymentTypes? DefaultPaymentType { get; set; }
  public long? Id { get; set; }
}