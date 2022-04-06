using LegacyFighter.Cabs.Entity;
using NodaTime;

namespace LegacyFighter.Cabs.Dto;

public class ClaimDto
{
  public ClaimDto(
    long? claimId,
    long? clientId,
    long? transitId,
    string reason,
    string incidentDescription,
    Instant creationDate,
    Instant? completionDate,
    Instant? changeDate,
    Claim.CompletionModes? completionMode,
    Claim.Statuses? status,
    string claimNo)
  {
    ClaimId = claimId;
    ClientId = clientId;
    TransitId = transitId;
    Reason = reason;
    IncidentDescription = incidentDescription;
    IsDraft = status == Claim.Statuses.Draft;
    CreationDate = creationDate;
    CompletionDate = completionDate;
    ChangeDate = changeDate;
    CompletionMode = completionMode;
    Status = status;
    ClaimNo = claimNo;
  }

  public ClaimDto(Claim claim) : this(
    claim.Id,
    claim.Owner.Id,
    claim.TransitId,
    claim.Reason,
    claim.IncidentDescription,
    claim.CreationDate,
    claim.CompletionDate,
    claim.ChangeDate,
    claim.CompletionMode,
    claim.Status,
    claim.ClaimNo)
  {
  }

  public ClaimDto()
  {

  }

  public Instant CreationDate { get; set; }
  public Instant? CompletionDate { get; set; }
  public Instant? ChangeDate { get; set; }
  public Claim.CompletionModes? CompletionMode { get; set; }
  public Claim.Statuses? Status { get; set; }
  public string ClaimNo { get; set; }
  public long? ClaimId { get; set; }
  public long? ClientId { get; set; }
  public long? TransitId { get; set; }
  public string Reason { get; set; }
  public string IncidentDescription { get; set; }
  public bool IsDraft { get; set; }
}