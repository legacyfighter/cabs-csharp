using System.Collections.Generic;
using LegacyFighter.Cabs.Contracts.Application.Acme.Dynamic;
using LegacyFighter.Cabs.Contracts.Application.Editor;
using LegacyFighter.Cabs.Contracts.Legacy;
using LegacyFighter.Cabs.Contracts.Model;
using LegacyFighter.Cabs.Contracts.Model.Content;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Acme;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Predicates.StateChange;
using LegacyFighter.CabsTests.Common;

namespace LegacyFighter.CabsTests.Contracts.Application.Dynamic;

public class AcmeContractManagerBasedOnDynamicStateModelTest
{
  private CabsApp _app = default!;
  private IDocumentEditor Editor => _app.DocumentEditor;
  private IDocumentResourceManager DocumentResourceManager => _app.DocumentResourceManager;
  private IUserRepository UserRepository => _app.UserRepository;

  private const string Content1 = "content 1";
  private const string Content2 = "content 2";
  private readonly ContentVersion _anyVersion = new("v1");

  private User _author = default!;
  private User _verifier = default!;

  private DocumentNumber _documentNumber = default!;
  private long? _headerId = default!;

  [SetUp]
  public async Task InitializeApp()
  {
    _app = CabsApp.CreateInstance();
    _author = await UserRepository.Save(new User());
    _verifier = await UserRepository.Save(new User());
    _documentNumber = default!;
    _headerId = default!;
  }

  [TearDown]
  public async Task DisposeOfApp()
  {
    await _app.DisposeAsync();
  }

  [Test]
  public async Task VerifierOtherThanAuthorCanVerify()
  {
    //given
    await CrateAcmeContract(_author);
    var contentId = await CommitContent(Content1);
    var result = await DocumentResourceManager.ChangeContent(_headerId, contentId);
    new DocumentOperationResultAssert(result).State(AcmeContractStateAssembler.Draft).Editable()
      .PossibleNextStates(AcmeContractStateAssembler.Verified, AcmeContractStateAssembler.Archived);
    //when
    result = await DocumentResourceManager.ChangeState(_headerId, AcmeContractStateAssembler.Verified, VerifierParam());
    //then
    new DocumentOperationResultAssert(result).State(AcmeContractStateAssembler.Verified).Editable()
      .PossibleNextStates(AcmeContractStateAssembler.Published, AcmeContractStateAssembler.Archived);
  }

  [Test]
  public async Task AuthorCanNotVerify()
  {
    //given
    await CrateAcmeContract(_author);
    var contentId = await CommitContent(Content1);
    var result = await DocumentResourceManager.ChangeContent(_headerId, contentId);
    new DocumentOperationResultAssert(result).State(AcmeContractStateAssembler.Draft);
    //when
    result = await DocumentResourceManager.ChangeState(_headerId, AcmeContractStateAssembler.Verified, AuthorParam());
    //then
    new DocumentOperationResultAssert(result).State(AcmeContractStateAssembler.Draft);
  }

  [Test]
  public async Task ChangingContentOfVerifiedMovesBackToDraft()
  {
    //given
    await CrateAcmeContract(_author);
    var contentId = await CommitContent(Content1);
    var result = await DocumentResourceManager.ChangeContent(_headerId, contentId);
    new DocumentOperationResultAssert(result).State(AcmeContractStateAssembler.Draft).Editable();

    result = await DocumentResourceManager.ChangeState(_headerId, AcmeContractStateAssembler.Verified, VerifierParam());
    new DocumentOperationResultAssert(result).State(AcmeContractStateAssembler.Verified).Editable();
    //when
    contentId = await CommitContent(Content2);
    result = await DocumentResourceManager.ChangeContent(_headerId, contentId);
    //then
    new DocumentOperationResultAssert(result).State(AcmeContractStateAssembler.Draft).Editable();
  }

  [Test]
  public async Task PublishedCanNotBeChanged()
  {
    //given
    await CrateAcmeContract(_author);
    var firstContentId = await CommitContent(Content1);
    var result = await DocumentResourceManager.ChangeContent(_headerId, firstContentId);
    new DocumentOperationResultAssert(result).State(AcmeContractStateAssembler.Draft).Editable();

    result = await DocumentResourceManager.ChangeState(_headerId, AcmeContractStateAssembler.Verified, VerifierParam());
    new DocumentOperationResultAssert(result).State(AcmeContractStateAssembler.Verified).Editable();

    result = await DocumentResourceManager.ChangeState(_headerId, AcmeContractStateAssembler.Published, EmptyParam());
    new DocumentOperationResultAssert(result).State(AcmeContractStateAssembler.Published).Uneditable();
    //when
    var newContentId = await CommitContent(Content2);
    result = await DocumentResourceManager.ChangeContent(_headerId, newContentId);
    //then
    new DocumentOperationResultAssert(result).State(AcmeContractStateAssembler.Published).Uneditable()
      .Content(firstContentId);
  }

  private async Task<ContentId> CommitContent(string content)
  {
    var doc = new DocumentDto(null, content, _anyVersion);
    var result = await Editor.Commit(doc);
    Assert.AreEqual(CommitResult.Results.Success, result.Result);
    return new ContentId(result.ContentId);
  }

  private async Task CrateAcmeContract(User user)
  {
    var result = await DocumentResourceManager.CreateDocument(user.Id);
    _documentNumber = result.DocumentNumber;
    _headerId = result.DocumentHeaderId;
  }

  private Dictionary<string, object> VerifierParam()
  {
    return new Dictionary<string, object>
    {
      [AuthorIsNotAVerifier.ParamVerifier] = _verifier.Id
    };
  }

  private Dictionary<string, object> AuthorParam()
  {
    return new Dictionary<string, object>
    {
      [AuthorIsNotAVerifier.ParamVerifier] = _author.Id
    };
  }

  private Dictionary<string, object> EmptyParam()
  {
    return new Dictionary<string, object>();
  }
}