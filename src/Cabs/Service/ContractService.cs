using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.Service;

public class ContractService : IContractService
{
  private readonly IContractRepository _contractRepository;
  private readonly IContractAttachmentRepository _contractAttachmentRepository;

  public ContractService(IContractRepository contractRepository, IContractAttachmentRepository contractAttachmentRepository)
  {
    _contractRepository = contractRepository;
    _contractAttachmentRepository = contractAttachmentRepository;
  }

  public async Task<Contract> CreateContract(ContractDto contractDto)
  {
    var contract = new Contract();
    contract.PartnerName = contractDto.PartnerName;
    var partnerContractsCount = (await _contractRepository.FindByPartnerName(contractDto.PartnerName)).Count + 1;
    contract.Subject = contractDto.Subject;
    contract.ContractNo = "C/" + partnerContractsCount + "/" + contractDto.PartnerName;
    return await _contractRepository.Save(contract);
  }

  public async Task AcceptContract(long? id)
  {
    var contract = await Find(id);
    var attachments = await _contractAttachmentRepository.FindByContract(contract);
    if (attachments.All(a=>a.Status == ContractAttachment.Statuses.AcceptedByBothSides))
    {
      contract.Status = Contract.Statuses.Accepted;
    }
    else
    {
      throw new InvalidOperationException("Not all attachments accepted by both sides");
    }
  }

  public async Task RejectContract(long? id)
  {
    var contract = await Find(id);
    contract.Status = Contract.Statuses.Rejected;
  }

  public async Task RejectAttachment(long? attachmentId)
  {
    var contractAttachment = await _contractAttachmentRepository.Find(attachmentId);
    contractAttachment.Status = ContractAttachment.Statuses.Rejected;
  }

  public async Task AcceptAttachment(long? attachmentId)
  {
    var contractAttachment = await _contractAttachmentRepository.Find(attachmentId);
    if (contractAttachment.Status == ContractAttachment.Statuses.AcceptedByOneSide ||
        contractAttachment.Status == ContractAttachment.Statuses.AcceptedByBothSides)
    {
      contractAttachment.Status = ContractAttachment.Statuses.AcceptedByBothSides;
    }
    else
    {
      contractAttachment.Status = ContractAttachment.Statuses.AcceptedByOneSide;
    }
  }

  public async Task<Contract> Find(long? id)
  {
    var contract = await _contractRepository.Find(id);
    if (contract == null)
    {
      throw new InvalidOperationException("Contract does not exist");
    }

    return contract;
  }

  public async Task<ContractDto> FindDto(long? id)
  {
    return new ContractDto(await Find(id));
  }

  public async Task<ContractAttachmentDto> ProposeAttachment(long? contractId, ContractAttachmentDto contractAttachmentDto)
  {
    var contract = await Find(contractId);
    var contractAttachment = new ContractAttachment
    {
      Contract = contract,
      Data = contractAttachmentDto.Data
    };
    await _contractAttachmentRepository.Save(contractAttachment);
    contract.Attachments.Add(contractAttachment);
    return new ContractAttachmentDto(contractAttachment);
  }

  public async Task RemoveAttachment(long? contractId, long? attachmentId)
  {
    //TODO sprawdzenie czy nalezy do kontraktu (JIRA: II-14455)
    await _contractAttachmentRepository.DeleteById(attachmentId);
  }
}