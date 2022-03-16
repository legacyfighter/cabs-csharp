using LegacyFighter.Cabs.Contracts.Model;
using LegacyFighter.Cabs.Contracts.Model.Content;

namespace LegacyFighter.Cabs.Contracts.Application.Acme.Dynamic;

public class DocumentOperationResult
{
  public enum Results
  {
    Success,
    Error
  }

  public DocumentOperationResult(
    Results result,
    long? documentHeaderId,
    DocumentNumber documentNumber,
    string stateName,
    ContentId contentId,
    Dictionary<string, List<string>> possibleTransitionsAndRules,
    bool contentChangePossible,
    string contentChangePredicate)
  {
    Result = result;
    DocumentHeaderId = documentHeaderId;
    DocumentNumber = documentNumber;
    StateName = stateName;
    ContentId = contentId;
    PossibleTransitionsAndRules = possibleTransitionsAndRules;
    IsContentChangePossible = contentChangePossible;
    ContentChangePredicate = contentChangePredicate;
  }

  public Dictionary<string, List<string>> PossibleTransitionsAndRules { get; }
  public string ContentChangePredicate { get; }
  public bool IsContentChangePossible { get; }
  public Results Result { get; }
  public string StateName { get; }
  public DocumentNumber DocumentNumber { get; }
  public long? DocumentHeaderId { get; }
  public ContentId ContentId { get; }
}