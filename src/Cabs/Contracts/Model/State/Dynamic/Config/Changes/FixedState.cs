using LegacyFighter.Cabs.Contracts.FunctionalInterfaces;

namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Changes;

public class FixedState : IFunction<State, State>
{
  private readonly string _stateName;

  public FixedState(string stateName)
  {
    _stateName = stateName;
  }

  public State Apply(State state)
  {
    return new State(_stateName);
  }
}