using LegacyFighter.Cabs.Contracts.Application.Acme.Straightforward;
using LegacyFighter.Cabs.Contracts.Application.Editor;
using LegacyFighter.Cabs.Contracts.Legacy;
using LegacyFighter.Cabs.Contracts.Model;
using LegacyFighter.Cabs.Contracts.Model.Content;
using LegacyFighter.Cabs.Contracts.Model.State.Straightforward.Acme;
using LegacyFighter.CabsTests.Common;

namespace LegacyFighter.CabsTests.Contracts.Application.Straightforward.Acme;

public class AcmeContractProcessBasedOnStraightforwardStateModelTest
{
  private CabsApp _app = default!;
  private IDocumentEditor Editor => _app.DocumentEditor;
  private IAcmeContractProcessBasedOnStraightforwardDocumentModel ContractProcess
    => _app.AcmeContractProcessBasedOnStraightforwardDocumentModel;
  private IUserRepository UserRepository => _app.UserRepository;

  private const string Content1 = "content 1";
  private const string Content2 = "content 2";
  private readonly ContentVersion _anyVersion = new("v1");
  
  private User _author = default!;
  private User _verifier = default!;
  private DocumentNumber _documentNumber = default!;
  private long? _headerId;

  [SetUp]
  public async Task InitializeApp()
  {
    _app = CabsApp.CreateInstance();
    _author = await UserRepository.Save(new User());
    _verifier = await UserRepository.Save(new User());
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
    await ContractProcess.ChangeContent(_headerId, contentId);
    //when
    var result = await ContractProcess.Verify(_headerId, _verifier.Id);
    //then
    new ContractResultAssert(result).State(new VerifiedState(_verifier.Id));
  }

  [Test]
  public async Task AuthorCanNotVerify()
  {
    //given
    await CrateAcmeContract(_author);
    var contentId = await CommitContent(Content1);
    await ContractProcess.ChangeContent(_headerId, contentId);
    //when
    var result = await ContractProcess.Verify(_headerId, _author.Id);
    //then
    new ContractResultAssert(result).State(new DraftState());
  }

  [Test]
  public async Task ChangingContentOfVerifiedMovesBackToDraft()
  {
    //given
    await CrateAcmeContract(_author);
    var contentId = await CommitContent(Content1);
    var result = await ContractProcess.ChangeContent(_headerId, contentId);
    new ContractResultAssert(result).State(new DraftState());

    result = await ContractProcess.Verify(_headerId, _verifier.Id);
    new ContractResultAssert(result).State(new VerifiedState(_verifier.Id));
    //when
    contentId = await CommitContent(Content2);
    //then
    result = await ContractProcess.ChangeContent(_headerId, contentId);
    new ContractResultAssert(result).State(new DraftState());
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
    var contractResult = await ContractProcess.CreateContract(user.Id);
    _documentNumber = contractResult.DocumentNumber;
    _headerId = contractResult.DocumentHeaderId;
  }
}