using LegacyFighter.Cabs.Contracts.Legacy;
using LegacyFighter.Cabs.Contracts.Model;
using LegacyFighter.Cabs.Contracts.Model.Content;
using LegacyFighter.Cabs.Contracts.Model.State.Straightforward.Acme;

namespace LegacyFighter.Cabs.Contracts.Application.Acme.Straightforward;

public class AcmeContractProcessBasedOnStraightforwardDocumentModel : IAcmeContractProcessBasedOnStraightforwardDocumentModel
{
  private readonly IUserRepository _userRepository;
  private readonly IDocumentHeaderRepository _documentHeaderRepository;
  private readonly AcmeStateFactory _stateFactory;

  public AcmeContractProcessBasedOnStraightforwardDocumentModel(
    IUserRepository userRepository,
    IDocumentHeaderRepository documentHeaderRepository,
    AcmeStateFactory stateFactory)
  {
    _userRepository = userRepository;
    _documentHeaderRepository = documentHeaderRepository;
    _stateFactory = stateFactory;
  }

  public async Task<ContractResult> CreateContract(long? authorId)
  {
    var author = await _userRepository.Find(authorId);

    var number = GenerateNumber();
    var header = new DocumentHeader(author.Id, number);

    await _documentHeaderRepository.Save(header);

    return new ContractResult(ContractResult.Results.Success, header.Id, number, header.StateDescriptor);
  }

  public async Task<ContractResult> Verify(long? headerId, long? verifierId)
  {
    var verifier = await _userRepository.Find(verifierId);
    //TODO user authorization

    var header = await _documentHeaderRepository.Find(headerId);

    var state = _stateFactory.Create(header);
    state = state.ChangeState(new VerifiedState(verifierId));

    await _documentHeaderRepository.Save(header);
    return new ContractResult(ContractResult.Results.Success, headerId, header.DocumentNumber,
      header.StateDescriptor);
  }

  public async Task<ContractResult> ChangeContent(long? headerId, ContentId contentVersion)
  {
    var header = await _documentHeaderRepository.Find(headerId);

    var state = _stateFactory.Create(header);
    state = state.ChangeContent(contentVersion);

    await _documentHeaderRepository.Save(header);
    return new ContractResult(ContractResult.Results.Success, headerId, header.DocumentNumber,
      header.StateDescriptor);
  }

  private DocumentNumber GenerateNumber()
  {
    return new DocumentNumber("nr: " + new Random().Next()); //TODO integrate with doc number generator
  }
}