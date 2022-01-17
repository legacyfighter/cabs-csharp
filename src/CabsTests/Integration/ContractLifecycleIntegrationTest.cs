using System;
using System.Linq;
using System.Text;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Service;
using LegacyFighter.CabsTests.Common;

namespace LegacyFighter.CabsTests.Integration;

public class ContractLifecycleIntegrationTest
{
  private CabsApp _app = default!;
  private IContractService ContractService => _app.ContractService;

  [SetUp]
  public void InitializeApp()
  {
    _app = CabsApp.CreateInstance();
  }

  [TearDown]
  public void DisposeOfApp()
  {
    _app.Dispose();
  }

  [Test]
  public async Task CanCreateContract()
  {
    //given
    var created = await CreateContract("partnerNameVeryUnique", "umowa o cenę");

    //when
    var loaded = await LoadContract(created.Id);

    //then
    Assert.AreEqual("partnerNameVeryUnique", loaded.PartnerName);
    Assert.AreEqual("umowa o cenę", loaded.Subject);
    Assert.AreEqual("C/1/partnerNameVeryUnique", loaded.ContractNo);
    Assert.AreEqual(Contract.Statuses.NegotiationsInProgress, loaded.Status);
    Assert.NotNull(loaded.Id);
    Assert.NotNull(loaded.CreationDate);
    Assert.NotNull(loaded.CreationDate);
    Assert.Null(loaded.ChangeDate);
    Assert.Null(loaded.AcceptedAt);
    Assert.Null(loaded.RejectedAt);
  }

  [Test]
  public async Task SecondContractForTheSamePartnerHasCorrectNo()
  {
    //given
    var first = await CreateContract("uniqueName", "umowa o cenę");

    //when
    var second = await CreateContract("uniqueName", "umowa o cenę");
    //then
    var firstLoaded = await LoadContract(first.Id);
    var secondLoaded = await LoadContract(second.Id);

    Assert.AreEqual("uniqueName", firstLoaded.PartnerName);
    Assert.AreEqual("uniqueName", secondLoaded.PartnerName);
    Assert.AreEqual("C/1/uniqueName", firstLoaded.ContractNo);
    Assert.AreEqual("C/2/uniqueName", secondLoaded.ContractNo);
  }

  [Test]
  public async Task CanAddAttachmentToContract()
  {
    //given
    var created = await CreateContract("partnerName", "umowa o cenę");
    //when
    await AddAttachmentToContract(created, "content");

    //then
    var loaded = await LoadContract(created.Id);
    Assert.AreEqual(1, loaded.Attachments.Count);
    CollectionAssert.AreEqual(Encoding.Default.GetBytes("content"), loaded.Attachments[0].Data);
    Assert.AreEqual(ContractAttachment.Statuses.Proposed, loaded.Attachments[0].Status);
  }

  [Test]
  public async Task CanRemoveAttachmentFromContract()
  {
    //given
    var created = await CreateContract("partnerName", "umowa o cenę");
    //and
    var attachment = await AddAttachmentToContract(created, "content");

    //when
    await RemoveAttachmentFromContract(created, attachment);

    //then
    var loaded = await LoadContract(created.Id);
    Assert.AreEqual(0, loaded.Attachments.Count);
  }

  [Test]
  public async Task CanAcceptAttachmentByOneSide()
  {
    //given
    var created = await CreateContract("partnerName", "umowa o cenę");
    //and
    var attachment = await AddAttachmentToContract(created, "content");

    //when
    await AcceptAttachment(attachment);

    //then
    var loaded = await LoadContract(created.Id);
    Assert.AreEqual(1, loaded.Attachments.Count);
    Assert.AreEqual(ContractAttachment.Statuses.AcceptedByOneSide, loaded.Attachments[0].Status);
  }

  [Test]
  public async Task CanAcceptAttachmentByTwoSides()
  {
    //given
    var created = await CreateContract("partnerName", "umowa o cenę");
    //and
    var attachment = await AddAttachmentToContract(created, "content");

    //when
    await AcceptAttachment(attachment);
    //and
    await AcceptAttachment(attachment);

    //then
    var loaded = await LoadContract(created.Id);
    Assert.AreEqual(1, loaded.Attachments.Count);
    Assert.AreEqual(ContractAttachment.Statuses.AcceptedByBothSides, loaded.Attachments[0].Status);
  }

  [Test]
  public async Task CanRejectAttachment()
  {
    //given
    var created = await CreateContract("partnerName", "umowa o cenę");
    //and
    var attachment = await AddAttachmentToContract(created, "content");

    //when
    await RejectAttachment(attachment);

    //then
    var loaded = await LoadContract(created.Id);
    Assert.AreEqual(1, loaded.Attachments.Count);
    Assert.AreEqual(ContractAttachment.Statuses.Rejected, loaded.Attachments[0].Status);
  }

  [Test]
  public async Task CanAcceptContractWhenAllAttachmentsAccepted()
  {
    //given
    var created = await CreateContract("partnerName", "umowa o cenę");
    //and
    var attachment = await AddAttachmentToContract(created, "content");
    //and
    await AcceptAttachment(attachment);
    await AcceptAttachment(attachment);

    //when
    await AcceptContract(created);

    //then
    var loaded = await LoadContract(created.Id);
    Assert.AreEqual(Contract.Statuses.Accepted, loaded.Status);
  }

  [Test]
  public async Task CanRejectContract()
  {
    //given
    var created = await CreateContract("partnerName", "umowa o cenę");
    //and
    var attachment = await AddAttachmentToContract(created, "content");
    //and
    await AcceptAttachment(attachment);
    await AcceptAttachment(attachment);

    //when
    await RejectContract(created);

    //then
    var loaded = await LoadContract(created.Id);
    Assert.AreEqual(Contract.Statuses.Rejected, loaded.Status);
  }

  [Test]
  public async Task CannotAcceptContractWhenNotAllAttachmentsAccepted()
  {
    //given
    var created = await CreateContract("partnerName", "umowa o cenę");
    //and
    var attachment = await AddAttachmentToContract(created, "content");
    //and
    await AcceptAttachment(attachment);

    //expect
    await this.Awaiting(_ => AcceptContract(created))
      .Should().ThrowExactlyAsync<InvalidOperationException>();
    var loaded = await LoadContract(created.Id);
    Assert.AreNotEqual(Contract.Statuses.Accepted, loaded.Status);
  }

  private async Task<ContractDto> LoadContract(long? id)
  {
    return await ContractService.FindDto(id);
  }

  private async Task<ContractDto> CreateContract(string partnerName, string subject)
  {
    var dto = new ContractDto
    {
      PartnerName = partnerName,
      Subject = subject
    };
    var contract = await ContractService.CreateContract(dto);
    return await LoadContract(contract.Id);
  }

  private async Task<ContractAttachmentDto> AddAttachmentToContract(ContractDto created, string content)
  {
    var contractAttachmentDto = new ContractAttachmentDto
    {
      Data = Encoding.Default.GetBytes(content)
    };
    return await ContractService.ProposeAttachment(created.Id, contractAttachmentDto);
  }

  private async Task RemoveAttachmentFromContract(ContractDto contract, ContractAttachmentDto attachment)
  {
    await ContractService.RemoveAttachment(contract.Id, attachment.Id);
  }

  private async Task AcceptAttachment(ContractAttachmentDto attachment)
  {
    await ContractService.AcceptAttachment(attachment.Id);
  }

  private async Task RejectAttachment(ContractAttachmentDto attachment)
  {
    await ContractService.RejectAttachment(attachment.Id);
  }

  private async Task AcceptContract(ContractDto contract)
  {
    await ContractService.AcceptContract(contract.Id);
  }

  private async Task RejectContract(ContractDto contract)
  {
    await ContractService.RejectContract(contract.Id);
  }
}