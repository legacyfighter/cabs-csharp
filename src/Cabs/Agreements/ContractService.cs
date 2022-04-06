namespace LegacyFighter.Cabs.Agreements;

public class ContractService : IContractService
{
  private readonly IContractRepository _contractRepository;
  private readonly IContractAttachmentDataRepository _contractAttachmentDataRepository;

  public ContractService(IContractRepository contractRepository, IContractAttachmentDataRepository contractAttachmentDataRepository)
  {
    _contractRepository = contractRepository;
    _contractAttachmentDataRepository = contractAttachmentDataRepository;
  }

  public async Task<ContractDto> CreateContract(ContractDto contractDto)
  {
    var partnerContractsCount = (await _contractRepository.FindByPartnerName(contractDto.PartnerName)).Count + 1;
    var contract = new Contract(contractDto.PartnerName, contractDto.Subject, "C/" + partnerContractsCount + "/" + contractDto.PartnerName);
    return await FindDto((await _contractRepository.Save(contract)).Id);
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
    var contractAttachmentNo = await _contractRepository.FindContractAttachmentNoById(attachmentId);
    contract.RejectAttachment(contractAttachmentNo);
  }

  public async Task AcceptAttachment(long? attachmentId)
  {
    var contract = await _contractRepository.FindByAttachmentId(attachmentId);
    var contractAttachmentNo = await _contractRepository.FindContractAttachmentNoById(attachmentId);
    contract.AcceptAttachment(contractAttachmentNo);
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
    var contract = await Find(id);
    return new ContractDto(contract, await _contractAttachmentDataRepository.FindByContractAttachmentNoIn(contract.AttachmentIds));
  }

  public async Task<ContractAttachmentDto> ProposeAttachment(long? contractId, ContractAttachmentDto contractAttachmentDto)
  {
    var contract = await Find(contractId);
    var contractAttachmentId = contract.ProposeAttachment().ContractAttachmentNo;
    var contractAttachmentData = new ContractAttachmentData(contractAttachmentId, contractAttachmentDto.Data);
    contract = await _contractRepository.Save(contract);
    return new ContractAttachmentDto(contract.FindAttachment(contractAttachmentId), await _contractAttachmentDataRepository.Save(contractAttachmentData));
  }

  public async Task RemoveAttachment(long? contractId, long? attachmentId)
  {
    //TODO sprawdzenie czy nalezy do kontraktu (JIRA: II-14455)
    var contract = await Find(contractId);
    var contractAttachmentNo = await _contractRepository.FindContractAttachmentNoById(attachmentId);
    contract.Remove(contractAttachmentNo);
    await _contractAttachmentDataRepository.DeleteByAttachmentId(attachmentId);
  }
}