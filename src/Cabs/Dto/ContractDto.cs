using LegacyFighter.Cabs.Entity;
using NodaTime;

namespace LegacyFighter.Cabs.Dto;

public class ContractDto
{
  public ContractDto()
  {
  }

  public ContractDto(Contract contract) 
  {
    ContractNo = contract.ContractNo;
    AcceptedAt = contract.AcceptedAt;
    RejectedAt = contract.RejectedAt;
    CreationDate = contract.CreationDate;
    ChangeDate = contract.ChangeDate;
    Status = contract.Status;
    PartnerName = contract.PartnerName;
    Subject = contract.Subject;
    foreach (var attachment in contract.Attachments) 
    {
      Attachments.Add(new ContractAttachmentDto(attachment));
    }
    Id = contract.Id;
  }

  public Instant CreationDate { set; get; }
  public Instant? AcceptedAt { set; get; }
  public Instant? RejectedAt { set; get; }
  public Instant? ChangeDate { set; get; }
  public Contract.Statuses Status { set; get; }
  public string ContractNo { set; get; }
  public string PartnerName { set; get; }
  public string Subject { set; get; }
  public long? Id { set; get; }
  public List<ContractAttachmentDto> Attachments { get; set; } = new();
}