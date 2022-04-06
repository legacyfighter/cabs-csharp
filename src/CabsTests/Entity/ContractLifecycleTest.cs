using System;
using LegacyFighter.Cabs.Agreements;
using static LegacyFighter.Cabs.Agreements.ContractAttachmentStatuses;

namespace LegacyFighter.CabsTests.Entity;

public class ContractLifecycleTest
{
  [Test]
  public void CanCreateContract()
  {
    //when
    var contract = CreateContract("partnerNameVeryUnique", "umowa o cenę");

    //then
    Assert.AreEqual("partnerNameVeryUnique", contract.PartnerName);
    Assert.AreEqual("umowa o cenę", contract.Subject);
    Assert.AreEqual(ContractStatuses.NegotiationsInProgress, contract.Status);
    Assert.NotNull(contract.CreationDate);
    Assert.NotNull(contract.CreationDate);
    Assert.Null(contract.ChangeDate);
    Assert.Null(contract.AcceptedAt);
    Assert.Null(contract.RejectedAt);
  }

  [Test]
  public void CanAddAttachmentToContract()
  {
    //given
    var contract = CreateContract("partnerName", "umowa o cenę");

    //when
    var contractAttachment = contract.ProposeAttachment();

    //then
    Assert.AreEqual(1, contract.AttachmentIds.Count);
    Assert.AreEqual(Proposed, contract.FindAttachment(contractAttachment.ContractAttachmentNo).Status);
  }

  [Test]
  public void CanRemoveAttachmentFromContract()
  {
    //given
    var contract = CreateContract("partnerName", "umowa o cenę");
    //and
    var attachment = contract.ProposeAttachment();

    //when
    contract.Remove(attachment.ContractAttachmentNo);

    //then
    Assert.AreEqual(0, contract.AttachmentIds.Count);
  }

  [Test]
  public void CanAcceptAttachmentByOneSide()
  {
    //given
    var contract = CreateContract("partnerName", "umowa o cenę");
    //and
    var attachment = contract.ProposeAttachment();

    //when
    contract.AcceptAttachment(attachment.ContractAttachmentNo);

    //then
    Assert.AreEqual(1, contract.AttachmentIds.Count);
    Assert.AreEqual(AcceptedByOneSide, contract.FindAttachment(attachment.ContractAttachmentNo).Status);
  }

  [Test]
  public void CanAcceptAttachmentByTwoSides()
  {
    //given
    var contract = CreateContract("partnerName", "umowa o cenę");
    //and
    var attachment = contract.ProposeAttachment();

    //when
    contract.AcceptAttachment(attachment.ContractAttachmentNo);
    //and
    contract.AcceptAttachment(attachment.ContractAttachmentNo);

    //then
    Assert.AreEqual(1, contract.AttachmentIds.Count);
    Assert.AreEqual(AcceptedByBothSides, contract.FindAttachment(attachment.ContractAttachmentNo).Status);
  }

  [Test]
  public void CanRejectAttachment()
  {
    //given
    var contract = CreateContract("partnerName", "umowa o cenę");
    //and
    var attachment = contract.ProposeAttachment();

    //when
    contract.RejectAttachment(attachment.ContractAttachmentNo);

    //then
    Assert.AreEqual(1, contract.AttachmentIds.Count);
    Assert.AreEqual(Rejected, contract.FindAttachment(attachment.ContractAttachmentNo).Status);
  }

  [Test]
  public void CanAcceptContractWhenAllAttachmentsAccepted()
  {
    //given
    var contract = CreateContract("partnerName", "umowa o cenę");
    //and
    var attachment = contract.ProposeAttachment();
    //and
    contract.AcceptAttachment(attachment.ContractAttachmentNo);
    contract.AcceptAttachment(attachment.ContractAttachmentNo);

    //when
    contract.Accept();

    //then
    Assert.AreEqual(ContractStatuses.Accepted, contract.Status);
  }

  [Test]
  public void CanRejectContract()
  {
    //given
    var contract = CreateContract("partnerName", "umowa o cenę");
    //and
    var attachment = contract.ProposeAttachment();
    //and
    contract.AcceptAttachment(attachment.ContractAttachmentNo);
    contract.AcceptAttachment(attachment.ContractAttachmentNo);

    //when
    contract.Reject();

    //then
    Assert.AreEqual(ContractStatuses.Rejected, contract.Status);
  }

  [Test]
  public void CannotAcceptContractWhenNotAllAttachmentsAccepted()
  {
    //given
    var contract = CreateContract("partnerName", "umowa o cenę");
    //and
    var attachment = contract.ProposeAttachment();
    //and
    contract.AcceptAttachment(attachment.ContractAttachmentNo);

    //expect
    contract.Invoking(c => c.Accept()).Should().ThrowExactly<InvalidOperationException>();
    Assert.AreNotEqual(ContractStatuses.Accepted, contract.Status);
  }


  private Contract CreateContract(string partnerName, string subject)
  {
    return new Contract(partnerName, subject, "no");
  }
}