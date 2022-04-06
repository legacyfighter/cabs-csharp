using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Agreements;

public class Contract : BaseEntity
{
  public Contract()
  {
  }

  public Contract(string partnerName, string subject, string contractNo) 
  {
    PartnerName = partnerName;
    Subject = subject;
    ContractNo = contractNo;
  }

  internal Instant CreationDate { get; } = SystemClock.Instance.GetCurrentInstant();
  internal Instant? AcceptedAt { get; private set; }
  internal Instant? RejectedAt { get; private set; }
  internal Instant? ChangeDate { get; private set; }
  internal ContractStatuses Status { get; private set; } = ContractStatuses.NegotiationsInProgress;
  internal string ContractNo { get; private set; }
  protected virtual ISet<ContractAttachment> Attachments { get; set; } = new HashSet<ContractAttachment>();
  internal string PartnerName { get; private set; }
  internal string Subject { get; private set; }

  internal List<Guid> AttachmentIds => Attachments
      .Select(a => a.ContractAttachmentNo)
      .ToList();

  internal ContractAttachment ProposeAttachment()
  {
    var contractAttachment = new ContractAttachment();
    contractAttachment.Contract = this;
    Attachments.Add(contractAttachment);
    return contractAttachment;
  }

  internal void Accept()
  {
    if (Attachments.All(a => a.Status == ContractAttachmentStatuses.AcceptedByBothSides))
    {
      Status = ContractStatuses.Accepted;
    }
    else
    {
      throw new InvalidOperationException("Not all attachments accepted by both sides");
    }
  }

  internal void Reject()
  {
    Status = ContractStatuses.Rejected;
  }

  internal void AcceptAttachment(Guid contractAttachmentNo)
  {
    var contractAttachment = FindAttachment(contractAttachmentNo);
    if (contractAttachment.Status
        is ContractAttachmentStatuses.AcceptedByOneSide
        or ContractAttachmentStatuses.AcceptedByBothSides)
    {
      contractAttachment.Status = ContractAttachmentStatuses.AcceptedByBothSides;
    }
    else
    {
      contractAttachment.Status = ContractAttachmentStatuses.AcceptedByOneSide;
    }
  }

  internal void RejectAttachment(Guid contractAttachmentNo) 
  {
    var contractAttachment = FindAttachment(contractAttachmentNo);
    contractAttachment.Status = ContractAttachmentStatuses.Rejected;
  }

  internal void Remove(Guid contractAttachmentNo) 
  {
    Attachments = Attachments
      .Where(attachment => attachment.ContractAttachmentNo != contractAttachmentNo)
      .ToHashSet();
  }

  internal ContractAttachment FindAttachment(Guid? attachmentNo) 
  {
    return Attachments.FirstOrDefault(a => a.ContractAttachmentNo == attachmentNo);
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(this, obj)) return true;
    return obj != null && Id != null && Id == (obj as Contract)?.Id;
  }

  public static bool operator ==(Contract left, Contract right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(Contract left, Contract right)
  {
    return !Equals(left, right);
  }
}