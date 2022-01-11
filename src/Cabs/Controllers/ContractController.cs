using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Service;
using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.Controllers;

[ApiController]
[Route("[controller]")]
public class ContractController 
{
  private readonly IContractService _contractService;

  public ContractController(IContractService contractService)
  {
    _contractService = contractService;
  }

  [HttpPost("/contracts/")]
  public async Task<ContractDto> Create([FromBody] ContractDto contractDto)
  {
    var created = await _contractService.CreateContract(contractDto);
    return new ContractDto(created);
  }


  [HttpGet("/contracts/{id}")]
  public async Task<ContractDto> Find(long? id)
  {
    var contract = await _contractService.FindDto(id);
    return contract;
  }

  [HttpPost("/contracts/{id}/attachment")]
  public async Task<ContractAttachmentDto> ProposeAttachment(
    long? id, [FromBody] ContractAttachmentDto contractAttachmentDto)
  {
    var dto = await _contractService.ProposeAttachment(id, contractAttachmentDto);
    return dto;
  }

  [HttpPost("/contracts/{contractId}/attachment/{attachmentId}/reject")]
  public async Task<IActionResult> RejectAttachment(long? contractId,
    long? attachmentId)
  {
    await _contractService.RejectAttachment(attachmentId);
    return new OkResult();
  }

  [HttpPost("/contracts/{contractId}/attachment/{attachmentId}/accept")]
  public async Task<IActionResult> AcceptAttachment(long? contractId,
    long? attachmentId)
  {
    await _contractService.AcceptAttachment(attachmentId);
    return new OkResult();
  }

  [HttpDelete("/contracts/{contractId}/attachment/{attachmentId}")]
  public async Task<IActionResult> RemoveAttachment(long? contractId,
    long? attachmentId)
  {
    await _contractService.RemoveAttachment(contractId, attachmentId);
    return new OkResult();
  }

  [HttpPost("/contracts/{id}/accept")]
  public async Task<IActionResult> AcceptContract(long? id)
  {
    await _contractService.AcceptContract(id);
    return new OkResult();
  }

  [HttpPost("/contracts/{id}/reject")]
  public async Task<IActionResult> RejectContract(long? id)
  {
    await _contractService.RejectContract(id);
    return new OkResult();
  }
}