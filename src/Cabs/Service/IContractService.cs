using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Service;

public interface IContractService
{
  Task<Contract> CreateContract(ContractDto contractDto);
  Task AcceptContract(long? id);
  Task RejectContract(long? id);
  Task RejectAttachment(long? attachmentId);
  Task AcceptAttachment(long? attachmentId);
  Task<Contract> Find(long? id);
  Task<ContractDto> FindDto(long? id);
  Task<ContractAttachmentDto> ProposeAttachment(long? contractId, ContractAttachmentDto contractAttachmentDto);
  Task RemoveAttachment(long? contractId, long? attachmentId);
}