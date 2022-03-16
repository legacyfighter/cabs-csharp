using System;
using LegacyFighter.Cabs.Contracts.Model;
using LegacyFighter.Cabs.Contracts.Model.Content;
using LegacyFighter.Cabs.Contracts.Model.State.Straightforward;
using LegacyFighter.Cabs.Contracts.Model.State.Straightforward.Acme;

namespace LegacyFighter.CabsTests.Contracts.Model.State.Straightforward;

public class AcmeContractTest
{
  private static readonly DocumentNumber AnyNumber = new("nr: 1");
  private const long AnyUser = 1L;
  private const long OtherUser = 2L;
  private static readonly ContentId AnyVersion = new(Guid.NewGuid());
  private static readonly ContentId OtherVersion = new(Guid.NewGuid());

  private BaseState? _state;

  [SetUp]
  public void SetUp()
  {
    _state = null;
  }

  [Test]
  public void OnlyDraftCanBeVerifiedByUserOtherThanCreator()
  {
    //given
    _state = Draft().ChangeContent(AnyVersion);
    //when
    _state = _state.ChangeState(new VerifiedState(OtherUser));
    //then
    Assert.IsInstanceOf<VerifiedState>(_state);
    Assert.AreEqual(OtherUser, _state.GetDocumentHeader().Verifier);
  }

  [Test]
  public void CanNotChangePublished()
  {
    //given
    _state = Draft().ChangeContent(AnyVersion).ChangeState(new VerifiedState(OtherUser))
      .ChangeState(new PublishedState());
    //when
    _state = _state.ChangeContent(OtherVersion);
    //then
    Assert.IsInstanceOf<PublishedState>(_state);
    Assert.AreEqual(AnyVersion, _state.GetDocumentHeader().ContentId);
  }

  [Test]
  public void ChangingVerifiedMovesToDraft()
  {
    //given
    _state = Draft().ChangeContent(AnyVersion);
    //when
    _state = _state.ChangeState(new VerifiedState(OtherUser)).ChangeContent(OtherVersion);
    //then
    Assert.IsInstanceOf<DraftState>(_state);
    Assert.AreEqual(OtherVersion, _state.GetDocumentHeader().ContentId);
  }

  private BaseState Draft()
  {
    var header = new DocumentHeader(AnyUser, AnyNumber);

    BaseState state = new DraftState();
    state.Init(header);

    return state;
  }
}