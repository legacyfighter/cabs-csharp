using LegacyFighter.Cabs.Contracts.Model.Content;

namespace LegacyFighter.Cabs.Contracts.Application.Acme.Straightforward;

public class ContractResult
{
  public enum Results
  {
    Failure,
    Success
  }

  public ContractResult(
    Results result,
    long? documentHeaderId,
    DocumentNumber documentNumber,
    string stateDescriptor)
  {
    Result = result;
    DocumentHeaderId = documentHeaderId;
    DocumentNumber = documentNumber;
    StateDescriptor = stateDescriptor;
  }

  public Results Result { get; }
  public DocumentNumber DocumentNumber { get; }
  public long? DocumentHeaderId { get; }
  public string StateDescriptor { get; }
}