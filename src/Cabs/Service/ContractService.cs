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
    var partnerContractsCount = (await _contractRepository.FindByPartnerName(contractDto.PartnerName)).Count + 1;
    var contract = new Contract(contractDto.PartnerName, contractDto.Subject, "C/" + partnerContractsCount + "/" + contractDto.PartnerName);
    return await _contractRepository.Save(contract);
  }

  public async Task AcceptContract(long? id)
  {
    var contract = await Find(id);
    contract.Accept();
  }

  public async Task RejectContract(long? id)
  {
    var contract = await Find(id);
    contract.Reject();
  }

  public async Task RejectAttachment(long? attachmentId)
  {
    var contract = await _contractRepository.FindByAttachmentId(attachmentId);
    contract.RejectAttachment(attachmentId);
  }

  public async Task AcceptAttachment(long? attachmentId)
  {
    var contract = await _contractRepository.FindByAttachmentId(attachmentId);
    contract.AcceptAttachment(attachmentId);
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
    return new ContractDto(await Find(id), await _contractAttachmentRepository.FindByContractId(id));
  }

  public async Task<ContractAttachmentDto> ProposeAttachment(long? contractId, ContractAttachmentDto contractAttachmentDto)
  {
    var contract = await Find(contractId);
    var contractAttachment = contract.ProposeAttachment(contractAttachmentDto.Data);
    return new ContractAttachmentDto(await _contractAttachmentRepository.Save(contractAttachment));
  }

  public async Task RemoveAttachment(long? contractId, long? attachmentId)
  {
    //TODO sprawdzenie czy nalezy do kontraktu (JIRA: II-14455)
    await _contractAttachmentRepository.DeleteById(attachmentId);
  }
}