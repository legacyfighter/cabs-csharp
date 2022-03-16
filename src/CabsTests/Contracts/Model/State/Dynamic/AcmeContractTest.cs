using System;
using LegacyFighter.Cabs.Contracts.Model;
using LegacyFighter.Cabs.Contracts.Model.Content;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Acme;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Events;

namespace LegacyFighter.CabsTests.Contracts.Model.State.Dynamic;

public class AcmeContractTest
{
  private static readonly DocumentNumber AnyNumber = new("nr: 1");
  private const long AnyUser = 1L;
  private const long OtherUser = 2L;
  private static readonly ContentId AnyVersion = new(Guid.NewGuid());
  private static readonly ContentId OtherVersion = new(Guid.NewGuid());

  private FakeDocumentPublisher _publisher = default!;

  private Cabs.Contracts.Model.State.Dynamic.State Draft()
  {
    var header = new DocumentHeader(AnyUser, AnyNumber);
    header.StateDescriptor = AcmeContractStateAssembler.Draft;
    _publisher = new FakeDocumentPublisher();

    var assembler = new AcmeContractStateAssembler(_publisher);
    var config = assembler.Assemble();
    var state = config.Recreate(header);

    return state;
  }

  [Test]
  public async Task DraftCanBeVerifiedByUserOtherThanCreator()
  {
    //given
    var state = Draft().ChangeContent(AnyVersion);
    //when
    state = await state.ChangeState(
      new ChangeCommand(AcmeContractStateAssembler.Verified).WithParam(AcmeContractStateAssembler.ParamVerifier,
        OtherUser));
    //then
    Assert.AreEqual(AcmeContractStateAssembler.Verified, state.StateDescriptor);
    Assert.AreEqual(OtherUser, state.DocumentHeader.Verifier);
  }

  [Test]
  public async Task CanNotChangePublished()
  {
    //given
    Cabs.Contracts.Model.State.Dynamic.State state = state = await (await Draft()
        .ChangeContent(AnyVersion)
        .ChangeState(new ChangeCommand(AcmeContractStateAssembler.Verified)
          .WithParam(AcmeContractStateAssembler.ParamVerifier, OtherUser)))
      .ChangeState(new ChangeCommand(AcmeContractStateAssembler.Published));

    _publisher.Contains<DocumentPublished>();
    _publisher.Reset();
    //when
    state = state.ChangeContent(OtherVersion);
    //then
    _publisher.NoEvents();
    Assert.AreEqual(AcmeContractStateAssembler.Published, state.StateDescriptor);
    Assert.AreEqual(AnyVersion, state.DocumentHeader.ContentId);
  }

  [Test]
  public async Task ChangingVerifiedMovesToDraft()
  {
    //given
    var state = await Draft().ChangeContent(AnyVersion)
      .ChangeState(
        new ChangeCommand(AcmeContractStateAssembler.Verified).WithParam(AcmeContractStateAssembler.ParamVerifier,
          OtherUser));
    //when
    state = state.ChangeContent(OtherVersion);
    //then
    Assert.AreEqual(AcmeContractStateAssembler.Draft, state.StateDescriptor);
    Assert.AreEqual(OtherVersion, state.DocumentHeader.ContentId);
  }


  [Test]
  public async Task CanChangeStateToTheSame()
  {
    var state = Draft().ChangeContent(AnyVersion);
    Assert.AreEqual(AcmeContractStateAssembler.Draft, state.StateDescriptor);
    await state.ChangeState(new ChangeCommand(AcmeContractStateAssembler.Draft));
    Assert.AreEqual(AcmeContractStateAssembler.Draft, state.StateDescriptor);

    state = await state.ChangeState(
      new ChangeCommand(AcmeContractStateAssembler.Verified).WithParam(AcmeContractStateAssembler.ParamVerifier,
        OtherUser));
    Assert.AreEqual(AcmeContractStateAssembler.Verified, state.StateDescriptor);
    state = await state.ChangeState(
      new ChangeCommand(AcmeContractStateAssembler.Verified).WithParam(AcmeContractStateAssembler.ParamVerifier,
        OtherUser));
    Assert.AreEqual(AcmeContractStateAssembler.Verified, state.StateDescriptor);

    state = await state.ChangeState(new ChangeCommand(AcmeContractStateAssembler.Published));
    Assert.AreEqual(AcmeContractStateAssembler.Published, state.StateDescriptor);
    state = await state.ChangeState(new ChangeCommand(AcmeContractStateAssembler.Published));
    Assert.AreEqual(AcmeContractStateAssembler.Published, state.StateDescriptor);

    state = await state.ChangeState(new ChangeCommand(AcmeContractStateAssembler.Archived));
    Assert.AreEqual(AcmeContractStateAssembler.Archived, state.StateDescriptor);
    state = await state.ChangeState(new ChangeCommand(AcmeContractStateAssembler.Archived));
    Assert.AreEqual(AcmeContractStateAssembler.Archived, state.StateDescriptor);
  }
}