using NodaTime;

namespace LegacyFighter.Cabs.Agreements;

public class ContractDto
{
  public ContractDto()
  {
  }

  public ContractDto(Contract contract, ISet<ContractAttachmentData> attachments) 
  {
    ContractNo = contract.ContractNo;
    AcceptedAt = contract.AcceptedAt;
    RejectedAt = contract.RejectedAt;
    CreationDate = contract.CreationDate;
    ChangeDate = contract.ChangeDate;
    Status = contract.Status;
    PartnerName = contract.PartnerName;
    Subject = contract.Subject;
    foreach (var attachmentData in attachments) 
    {
      var contractAttachmentNo = attachmentData.ContractAttachmentNo;
      var attachment = contract.FindAttachment(contractAttachmentNo);
      Attachments.Add(new ContractAttachmentDto(attachment, attachmentData));
    }
    Id = contract.Id;
  }

  public Instant CreationDate { set; get; }
  public Instant? AcceptedAt { set; get; }
  public Instant? RejectedAt { set; get; }
  public Instant? ChangeDate { set; get; }
  public ContractStatuses Status { set; get; }
  public string ContractNo { set; get; }
  public string PartnerName { set; get; }
  public string Subject { set; get; }
  public long? Id { set; get; }
  public List<ContractAttachmentDto> Attachments { get; set; } = new();
}