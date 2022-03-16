using LegacyFighter.Cabs.Contracts.Model;

namespace LegacyFighter.Cabs.Contracts.Application.Acme.Straightforward;

public interface IAcmeContractProcessBasedOnStraightforwardDocumentModel
{
  Task<ContractResult> CreateContract(long? authorId);
  Task<ContractResult> Verify(long? headerId, long? verifierId);
  Task<ContractResult> ChangeContent(long? headerId, ContentId contentVersion);
}
