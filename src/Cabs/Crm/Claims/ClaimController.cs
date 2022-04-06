using LegacyFighter.Cabs.Common;
using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.Crm.Claims;

[ApiController]
[Route("[controller]")]
public class ClaimController
{
  private readonly IClaimService _claimService;
  private readonly ITransactions _transactions;

  public ClaimController(IClaimService claimService, ITransactions transactions)
  {
    _claimService = claimService;
    _transactions = transactions;
  }

  [HttpPost("/claims/createDraft")]
  public async Task<ClaimDto> Create([FromBody] ClaimDto claimDto)
  {
    var created = await _claimService.Create(claimDto);
    return ToDto(created);
  }

  [HttpPost("/claims/send")]
  public async Task<ClaimDto> SendNew([FromBody] ClaimDto claimDto)
  {
    claimDto.IsDraft = false;
    var claim = await _claimService.Create(claimDto);
    return ToDto(claim);
  }

  [HttpPost("/claims/{id}/markInProcess")]
  public async Task<ClaimDto> MarkAsInProcess(long? id)
  {
    var claim = await _claimService.SetStatus(Statuses.InProcess, id);
    return ToDto(claim);
  }

  [HttpGet("/claims/{id}")]
  public async Task<ClaimDto> Find(long id)
  {
    await using var tx = await _transactions.BeginTransaction();
    var claim = await _claimService.Find(id);
    var dto = ToDto(claim);
    await tx.Commit();
    return dto;
  }

  [HttpPost("/claims/{id}")]
  public async Task<ClaimDto> TryToAutomaticallyResolve(long id)
  {
    var claim = await _claimService.TryToResolveAutomatically(id);
    return ToDto(claim);
  }

  private ClaimDto ToDto(Claim claim)
  {
    return new ClaimDto(claim);
  }
}