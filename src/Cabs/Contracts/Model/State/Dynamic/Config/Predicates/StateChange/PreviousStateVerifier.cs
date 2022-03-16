using LegacyFighter.Cabs.Contracts.FunctionalInterfaces;

namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Predicates.StateChange;

public class PreviousStateVerifier : IBiFunction<State, ChangeCommand, bool>
{
  private readonly string _stateDescriptor;

  public PreviousStateVerifier(string stateDescriptor)
  {
    _stateDescriptor = stateDescriptor;
  }

  public bool Apply(State state, ChangeCommand command)
  {
    return state.StateDescriptor.Equals(_stateDescriptor);
  }
}