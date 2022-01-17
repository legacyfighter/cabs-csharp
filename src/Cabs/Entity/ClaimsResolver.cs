using System.Text.Json;
using LegacyFighter.Cabs.Common;

namespace LegacyFighter.Cabs.Entity;

public class ClaimsResolver : BaseEntity
{
  public class Result
  {
    public WhoToAsk WhoToAsk { get; set; }
    public Claim.Statuses Decision { get; set; }

    internal Result(WhoToAsk whoToAsk, Claim.Statuses decision)
    {
      WhoToAsk = whoToAsk;
      Decision = decision;
    }
  }

  public ClaimsResolver(long? clientId)
  {
    ClientId = clientId;
  }

  public ClaimsResolver()
  {
  }

  public enum WhoToAsk
  {
    AskDriver, AskClient, AskNoOne
  }

  internal long? ClientId { get; }

  private string ClaimedTransitsIds { get; set; }

  public Result Resolve(Claim claim, double automaticRefundForVipThreshold, int numberOfTransits, double noOfTransitsForClaimAutomaticRefund)
  {
    var transitId = claim.Transit.Id;
    if (GetClaimedTransitsIds().Contains(transitId))
    {
      return new Result(WhoToAsk.AskNoOne, Claim.Statuses.Escalated);
    }
    AddNewClaimFor(claim.Transit);
    if (NumberOfClaims() <= 3)
    {
      return new Result(WhoToAsk.AskNoOne, Claim.Statuses.Refunded);
    }
    if (claim.Owner.Type == Client.Types.Vip)
    {
      if (claim.Transit.Price.IntValue < automaticRefundForVipThreshold)
      {
        return new Result(WhoToAsk.AskNoOne, Claim.Statuses.Refunded);
      }
      else
      {
        return new Result(WhoToAsk.AskDriver, Claim.Statuses.Escalated);
      }
    }
    else
    {
      if (numberOfTransits >= noOfTransitsForClaimAutomaticRefund)
      {
        if (claim.Transit.Price.IntValue < automaticRefundForVipThreshold)
        {
          return new Result(WhoToAsk.AskNoOne, Claim.Statuses.Refunded);
        }
        else
        {
          return new Result(WhoToAsk.AskClient, Claim.Statuses.Escalated);
        }
      }
      else
      {
        return new Result(WhoToAsk.AskDriver, Claim.Statuses.Escalated);
      }
    }
  }

  private void AddNewClaimFor(Transit transit)
  {
    var transitsIds = GetClaimedTransitsIds();
    transitsIds.Add(transit.Id);
    ClaimedTransitsIds = JsonMapper.Serialize(transitsIds);
  }

  private ISet<long?> GetClaimedTransitsIds()
  {
    return JsonMapper.Deserialize(ClaimedTransitsIds);
  }

  private int NumberOfClaims()
  {
    return GetClaimedTransitsIds().Count;
  }

}

internal static class JsonMapper
{
  public static ISet<long?> Deserialize(string json)
  {
    if (json == null)
    {
      return new HashSet<long?>();
    }

    return JsonSerializer.Deserialize<ISet<long?>>(json);
  }

  public static string Serialize(ISet<long?> transitsIds)
  {
    return JsonSerializer.Serialize(transitsIds);
  }

}
