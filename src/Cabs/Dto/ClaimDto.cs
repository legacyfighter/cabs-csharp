using LegacyFighter.Cabs.Entity;
using NodaTime;

namespace LegacyFighter.Cabs.Dto;

public class ClaimDto
{
  public ClaimDto(Claim claim)
  {
    if (claim.Status == Claim.Statuses.Draft)
    {
      IsDraft = true;
    }
    else
    {
      IsDraft = false;
    }

    ClaimId = claim.Id;
    Reason = claim.Reason;
    IncidentDescription = claim.IncidentDescription;
    TransitId = claim.Transit.Id;
    ClientId = claim.Owner.Id;
    CompletionDate = claim.CompletionDate;
    ChangeDate = claim.ChangeDate;
    ClaimNo = claim.ClaimNo;
    Status = claim.Status;
    CompletionMode = claim.CompletionMode;
    CreationDate = claim.CreationDate;
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