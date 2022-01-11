using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public interface IClientService
{
  Task<Client> RegisterClient(string name, string lastName, Client.Types? type, Client.PaymentTypes? paymentType);
  Task ChangeDefaultPaymentType(long? clientId, Client.PaymentTypes? paymentType);
  Task UpgradeToVip(long? clientId);
  Task DowngradeToRegular(long? clientId);
  Task<ClientDto> Load(long? id);
}