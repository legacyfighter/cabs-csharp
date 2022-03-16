using LegacyFighter.Cabs.Contracts.Application.Acme.Straightforward;
using LegacyFighter.Cabs.Contracts.Model.State.Straightforward;

namespace LegacyFighter.CabsTests.Contracts.Application.Straightforward.Acme;

public class ContractResultAssert
{
  private readonly ContractResult _result;

  public ContractResultAssert(ContractResult result)
  {
    _result = result;
    Assert.AreEqual(ContractResult.Results.Success, result.Result);
  }

  public ContractResultAssert State(BaseState state)
  {
    Assert.AreEqual(state.StateDescriptor, _result.StateDescriptor);
    return this;
  }
}