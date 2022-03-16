using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Repair.Legacy.Parts;

namespace LegacyFighter.Cabs.Repair.Api;

public class ResolveResult
{
  public enum Statuses
  {
    Success,
    Error
  }

  public ResolveResult(Statuses status, Guid handlingParty, Money totalCost, ISet<Part> acceptedParts)
  {
    Status = status;
    HandlingParty = handlingParty;
    TotalCost = totalCost;
    AcceptedParts = acceptedParts;
  }

  public ResolveResult(Statuses status)
  {
    Status = status;
  }

  public Guid HandlingParty { get; }
  public Money TotalCost { get; }
  public Statuses Status { get; }
  public ISet<Part> AcceptedParts { get; }
}