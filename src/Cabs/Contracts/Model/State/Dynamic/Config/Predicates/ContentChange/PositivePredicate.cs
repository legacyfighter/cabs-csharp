using LegacyFighter.Cabs.Contracts.FunctionalInterfaces;

namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Predicates.ContentChange;

public class PositivePredicate : IPredicate<State>
{
  public bool Test(State state)
  {
    return true;
  }
}