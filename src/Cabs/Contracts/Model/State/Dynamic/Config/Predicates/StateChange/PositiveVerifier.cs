using LegacyFighter.Cabs.Contracts.FunctionalInterfaces;

namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Predicates.StateChange;

public class PositiveVerifier : IBiFunction<State, ChangeCommand, bool>
{
  public bool Apply(State state, ChangeCommand command)
  {
    return true;
  }
}