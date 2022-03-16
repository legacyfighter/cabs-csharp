using LegacyFighter.Cabs.Contracts.FunctionalInterfaces;

namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Predicates.StateChange;

public class ContentNotEmptyVerifier : IBiFunction<State, ChangeCommand, bool>
{
  public bool Apply(State state, ChangeCommand command)
  {
    return state.DocumentHeader.ContentId != null;
  }
}