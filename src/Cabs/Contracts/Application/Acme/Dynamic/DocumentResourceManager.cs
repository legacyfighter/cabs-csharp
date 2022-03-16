
using LegacyFighter.Cabs.Contracts.Legacy;
using LegacyFighter.Cabs.Contracts.Model;
using LegacyFighter.Cabs.Contracts.Model.Content;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Acme;

namespace LegacyFighter.Cabs.Contracts.Application.Acme.Dynamic;

public class DocumentResourceManager : IDocumentResourceManager
{
  private readonly IDocumentHeaderRepository _documentHeaderRepository;
  private readonly AcmeContractStateAssembler _assembler;
  private readonly IUserRepository _userRepository;

  public DocumentResourceManager(
    IDocumentHeaderRepository documentHeaderRepository,
    AcmeContractStateAssembler assembler,
    IUserRepository userRepository)
  {
    _documentHeaderRepository = documentHeaderRepository;
    _assembler = assembler;
    _userRepository = userRepository;
  }

  public async Task ChangeContent()
  {
  }

  public async Task<DocumentOperationResult> CreateDocument(long? authorId)
  {
    var author = await _userRepository.Find(authorId);

    var number = GenerateNumber();
    var documentHeader = new DocumentHeader(author.Id, number);

    var stateConfig = _assembler.Assemble();
    var state = stateConfig.Begin(documentHeader);

    await _documentHeaderRepository.Save(documentHeader);

    return GenerateDocumentOperationResult(DocumentOperationResult.Results.Success, state);
  }

  public async Task<DocumentOperationResult> ChangeState(long? documentId, string desiredState, Dictionary<string, object> @params)
  {
    var documentHeader = await _documentHeaderRepository.Find(documentId);
    var stateConfig = _assembler.Assemble();
    var state = stateConfig.Recreate(documentHeader);

    state = await state.ChangeState(new ChangeCommand(desiredState, @params));

    await _documentHeaderRepository.Save(documentHeader);

    return GenerateDocumentOperationResult(DocumentOperationResult.Results.Success, state);
  }

  public async Task<DocumentOperationResult> ChangeContent(long? headerId, ContentId contentVersion)
  {
    var documentHeader = await _documentHeaderRepository.Find(headerId);
    var stateConfig = _assembler.Assemble();
    var state = stateConfig.Recreate(documentHeader);
    state = state.ChangeContent(contentVersion);

    await _documentHeaderRepository.Save(documentHeader);
    return GenerateDocumentOperationResult(DocumentOperationResult.Results.Success, state);
  }

  private DocumentOperationResult GenerateDocumentOperationResult(DocumentOperationResult.Results result, State state)
  {
    return new DocumentOperationResult(result, state.DocumentHeader.Id,
      state.DocumentHeader.DocumentNumber, state.StateDescriptor,
      state.DocumentHeader.ContentId,
      ExtractPossibleTransitionsAndRules(state),
      state.IsContentEditable(),
      ExtractContentChangePredicate(state));
  }

  private string ExtractContentChangePredicate(State state)
  {
    if (state.IsContentEditable())
    {
      return state.ContentChangePredicate.GetType().Name;
    }

    return null;
  }

  private Dictionary<string, List<string>> ExtractPossibleTransitionsAndRules(State state)
  {
    var transitionsAndRules = new Dictionary<string, List<string>>();

    var stateChangePredicates = state.StateChangePredicates;
    foreach (var s in stateChangePredicates.Keys)
    {
      //transition to self is not important
      if (s.Equals(state))
      {
        continue;
      }

      var predicates = stateChangePredicates[s];
      var ruleNames = new List<string>();
      foreach (var predicate in predicates)
      {
        ruleNames.Add(predicate.GetType().Name);
      }

      transitionsAndRules[s.StateDescriptor] = ruleNames;
    }

    return transitionsAndRules;
  }

  private DocumentNumber GenerateNumber()
  {
    return new DocumentNumber("nr: " + new Random().Next()); //TODO integrate with doc number generator
  }
}