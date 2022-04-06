using System.Text.Json;
using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Crm.Claims;

public class ClaimsResolver : BaseEntity
{
  public class Result
  {
    public WhoToAsk WhoToAsk { get; set; }
    public Statuses Decision { get; set; }

    internal Result(WhoToAsk whoToAsk, Statuses decision)
    {
      WhoToAsk = whoToAsk;
      Decision = decision;
    }
  }

  internal ClaimsResolver(long? clientId)
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

  internal Result Resolve(Claim claim, Client.Types? clientType, double automaticRefundForVipThreshold, int numberOfTransits, double noOfTransitsForClaimAutomaticRefund)
  {
    var transitId = claim.TransitId;
    if (GetClaimedTransitsIds().Contains(transitId))
    {
      return new Result(WhoToAsk.AskNoOne, Statuses.Escalated);
    }
    AddNewClaimFor(claim.TransitId);
    if (NumberOfClaims() <= 3)
    {
      return new Result(WhoToAsk.AskNoOne, Statuses.Refunded);
    }
    if (clientType == Client.Types.Vip)
    {
      if (claim.TransitPrice.IntValue < automaticRefundForVipThreshold)
      {
        return new Result(WhoToAsk.AskNoOne, Statuses.Refunded);
      }
      else
      {
        return new Result(WhoToAsk.AskDriver, Statuses.Escalated);
      }
    }
    else
    {
      if (numberOfTransits >= noOfTransitsForClaimAutomaticRefund)
      {
        if (claim.TransitPrice.IntValue < automaticRefundForVipThreshold)
        {
          return new Result(WhoToAsk.AskNoOne, Statuses.Refunded);
        }
        else
        {
          return new Result(WhoToAsk.AskClient, Statuses.Escalated);
        }
      }
      else
      {
        return new Result(WhoToAsk.AskDriver, Statuses.Escalated);
      }
    }
  }

  private void AddNewClaimFor(long? transitId)
  {
    var transitsIds = GetClaimedTransitsIds();
    transitsIds.Add(transitId);
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