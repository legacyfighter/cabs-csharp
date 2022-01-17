using LegacyFighter.Cabs.Common;
using NodaTime;

namespace LegacyFighter.Cabs.Entity;

public class Contract : BaseEntity
{

  public enum Statuses
  {
    NegotiationsInProgress,
    Rejected,
    Accepted
  }

  public Contract()
  {
  }

  public Contract(string partnerName, string subject, string contractNo) 
  {
    PartnerName = partnerName;
    Subject = subject;
    ContractNo = contractNo;
  }

  public Instant CreationDate { get; } = SystemClock.Instance.GetCurrentInstant();
  public Instant? AcceptedAt { get; private set; }
  public Instant? RejectedAt { get; private set; }
  public Instant? ChangeDate { get; private set; }
  public Statuses Status { get; private set; } = Statuses.NegotiationsInProgress;
  public string ContractNo { get; private set; }
  protected virtual ISet<ContractAttachment> Attachments { get; set; } = new HashSet<ContractAttachment>();
  public string PartnerName { get; private set; }
  public string Subject { get; private set; }

  public List<Guid> AttachmentIds => Attachments
      .Select(a => a.ContractAttachmentNo)
      .ToList();

  public ContractAttachment ProposeAttachment()
  {
    var contractAttachment = new ContractAttachment();
    contractAttachment.Contract = this;
    Attachments.Add(contractAttachment);
    return contractAttachment;
  }

  public void Accept()
  {
    if (Attachments.All(a => a.Status == ContractAttachment.Statuses.AcceptedByBothSides))
    {
      Status = Statuses.Accepted;
    }
    else
    {
      throw new InvalidOperationException("Not all attachments accepted by both sides");
    }
  }

  public void Reject()
  {
    Status = Statuses.Rejected;
  }

  public void AcceptAttachment(Guid contractAttachmentNo)
  {
    var contractAttachment = FindAttachment(contractAttachmentNo);
    if (contractAttachment.Status
        is ContractAttachment.Statuses.AcceptedByOneSide
        or ContractAttachment.Statuses.AcceptedByBothSides)
    {
      contractAttachment.Status = ContractAttachment.Statuses.AcceptedByBothSides;
    }
    else
    {
      contractAttachment.Status = ContractAttachment.Statuses.AcceptedByOneSide;
    }
  }

  public void RejectAttachment(Guid contractAttachmentNo) 
  {
    var contractAttachment = FindAttachment(contractAttachmentNo);
    contractAttachment.Status = ContractAttachment.Statuses.Rejected;
  }

  public void Remove(Guid contractAttachmentNo) 
  {
    Attachments = Attachments
      .Where(attachment => attachment.ContractAttachmentNo != contractAttachmentNo)
      .ToHashSet();
  }

  public ContractAttachment FindAttachment(Guid? attachmentNo) 
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