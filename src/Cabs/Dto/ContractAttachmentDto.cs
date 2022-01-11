using LegacyFighter.Cabs.Entity;
using NodaTime;

namespace LegacyFighter.Cabs.Dto;

public class ContractAttachmentDto
{

  public ContractAttachmentDto()
  {
  }


  public ContractAttachmentDto(ContractAttachment attachment)
  {
    Id = attachment.Id;
    Data = attachment.Data;
    ContractId = attachment.Contract.Id;
    CreationDate = attachment.CreationDate;
    RejectedAt = attachment.RejectedAt;
    AcceptedAt = attachment.AcceptedAt;
    ChangeDate = attachment.ChangeDate;
    Status = attachment.Status;
  }

  public long? Id { get; set; }
  public long? ContractId { get; set; }
  public byte[] Data { get; set; }
  public Instant CreationDate { get; set; }
  public Instant AcceptedAt { get; set; }
  public Instant RejectedAt { get; set; }
  public Instant ChangeDate { get; set; }
  public ContractAttachment.Statuses Status { get; set; }
}