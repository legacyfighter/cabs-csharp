using LegacyFighter.Cabs.Contracts.FunctionalInterfaces;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Predicates.ContentChange;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Predicates.StateChange;

namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic;

public class StateBuilder : IStateConfig
{
  //last step of the Builder - because it is special
  public class FinalStateConfig
  {
    private readonly State _state;

    internal FinalStateConfig(State state)
    {
      _state = state;
    }

    /// <summary>
    /// Adds an operation to be performed if state have changed
    /// </summary>
    public FinalStateConfig Action(IBiFunction<DocumentHeader, ChangeCommand, Task> action)
    {
      _state.AddAfterStateChangeAction(action);
      return this;
    }
  }

  /// <summary>
  /// This <see cref="StateBuilder"/> state, that depends on method call
  /// </summary>
  private enum Modes
  {
    /// <summary>
    /// Rules for state transition <see cref="Check"/> method called or <see cref="From"/>  method called
    /// </summary>
    StateChange,

    /// <summary>
    /// Rules for content change <see cref="WhenContentChanged"/> method called
    /// </summary>
    ContentChange
  }

  private Modes? _mode;

  //all states configured so far
  private readonly Dictionary<string, State> _states = new();

  //below is the current state of the builder, gathered whit assembling methods, current state is reset in to() method
  private State _fromState;
  private State _initialState;
  private List<IBiFunction<State, ChangeCommand, bool>> _predicates;

  //========= methods for application layer - business process

  public State Begin(DocumentHeader header)
  {
    header.StateDescriptor = _initialState.StateDescriptor;
    return Recreate(header);
  }

  public State Recreate(DocumentHeader header)
  {
    var state = _states[header.StateDescriptor];
    state.Init(header);
    return state;
  }

  //======= methods for assembling process

  /// <summary>
  /// Similar to the <see cref="From"/> method, but marks initial state
  /// </summary>
  public StateBuilder BeginWith(string stateName)
  {
    if (_initialState != null)
      throw new InvalidOperationException("Initial state already set to: " + _initialState.StateDescriptor);

    var config = From(stateName);
    _initialState = _fromState;
    return config;
  }

  /// <summary>
  /// Begins a rule sequence with a beginning state
  /// </summary>
  public StateBuilder From(string stateName)
  {
    _mode = Modes.StateChange;
    _predicates = new List<IBiFunction<State, ChangeCommand, bool>>();
    _fromState = GetOrPut(stateName);
    return this;
  }

  /// <summary>
  /// Adds a rule to the current sequence
  /// </summary>
  public StateBuilder Check(IBiFunction<State, ChangeCommand, bool> checkingFunction)
  {
    _mode = Modes.StateChange;
    _predicates.Add(checkingFunction);
    return this;
  }

  /// <summary>
  /// Ends a rule sequence with a destination state
  /// </summary>
  public FinalStateConfig To(string stateName)
  {
    var toState = GetOrPut(stateName);

    switch (_mode)
    {
      case Modes.StateChange:
        _predicates.Add(new PreviousStateVerifier(_fromState.StateDescriptor));
        _fromState.AddStateChangePredicates(toState, _predicates);
        break;
      case Modes.ContentChange:
        _fromState.AfterContentChangeState = toState;
        toState.ContentChangePredicate = new PositivePredicate();
        break;
    }

    _predicates = null;
    _fromState = null;
    _mode = null;

    return new FinalStateConfig(toState);
  }

  /// <summary>
  /// Adds a rule of state change after a content change
  /// </summary>
  public StateBuilder WhenContentChanged()
  {
    _mode = Modes.ContentChange;
    return this;
  }

  private State GetOrPut(string stateName)
  {
    if (!_states.ContainsKey(stateName))
      _states[stateName] = new State(stateName);
    return _states[stateName];
  }
}