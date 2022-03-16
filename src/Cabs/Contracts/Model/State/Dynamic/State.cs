using Core.Maybe;
using LegacyFighter.Cabs.Contracts.FunctionalInterfaces;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Predicates.ContentChange;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Predicates.StateChange;

namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic;

public class State
{
  //before: GetType().Name
  /// <summary>
  /// Unique name of a state
  /// </summary>
  public string StateDescriptor { get; }

  //TODO consider to get rid of this stateful object and transform State to reusable logic
  public DocumentHeader DocumentHeader { get; private set; }

  //TODO consider merging contentChangePredicate and afterContentChangeState int one function

  //before: abstract CanChangeContent()

  /// <summary>
  /// predicates tested if content can be changed
  /// </summary>
  public IPredicate<State> ContentChangePredicate { get; internal set; } = new NegativePredicate(); //default

  //before: abstract StateAfterContentChange()

  /// <summary>
  /// state after content change - may be the same as before content change
  /// </summary>
  internal State AfterContentChangeState { get; set; }

  //before: abstract CanChangeFrom(state)

  /// <summary>
  /// possible transitions to other states with rules that need to be tested to determine if transition is legal
  /// </summary>
  public Dictionary<State, List<IBiFunction<State, ChangeCommand, bool>>> StateChangePredicates { get; } = new();

  //before: abstract Acquire()
  /// <summary>
  /// actions that may be needed to perform while transition to the next state
  /// </summary>
  private readonly List<IBiFunction<DocumentHeader, ChangeCommand, Task>> _afterStateChangeActions = new();

  public bool IsContentEditable()
  {
    return AfterContentChangeState != null;
  }

  public State(string stateDescriptor)
  {
    StateDescriptor = stateDescriptor;
    AddStateChangePredicates(this,
      new List<IBiFunction<State, ChangeCommand, bool>> { new PositiveVerifier() }); //change to self is always possible
  }

  /// <summary>
  /// initial bounding with a document header
  /// </summary>
  public void Init(DocumentHeader documentHeader)
  {
    DocumentHeader = documentHeader;
    documentHeader.StateDescriptor = StateDescriptor;
  }

  public State ChangeContent(ContentId currentContent)
  {
    if (!IsContentEditable())
      return this;

    var newState = AfterContentChangeState; //local variable just to focus attention
    if (newState.ContentChangePredicate.Test(this))
    {
      newState.Init(DocumentHeader);
      DocumentHeader.ChangeCurrentContent(currentContent);
      return newState;
    }

    return this;
  }


  public async Task<State> ChangeState(ChangeCommand command)
  {
    var desiredState = Find(command.DesiredState);
    if (desiredState == null)
      return this;

    var predicates = StateChangePredicates.Lookup(desiredState)
      .OrElse(new List<IBiFunction<State, ChangeCommand, bool>>());

    if (predicates.All(e => e.Apply(this, command)))
    {
      desiredState.Init(DocumentHeader);
      foreach (var action in desiredState._afterStateChangeActions)
      {
        await action.Apply(DocumentHeader, command);
      }
      return desiredState;
    }

    return this;
  }

  public override string ToString()
  {
    return "State{" +
           "stateDescriptor='" + StateDescriptor + '\'' +
           '}';
  }

  internal void AddStateChangePredicates(State toState,
    List<IBiFunction<State, ChangeCommand, bool>> predicatesToAdd)
  {
    if (StateChangePredicates.ContainsKey(toState))
    {
      var predicates = StateChangePredicates[toState];
      predicates.AddRange(predicatesToAdd);
    }
    else
    {
      StateChangePredicates[toState] = predicatesToAdd;
    }
  }

  internal void AddAfterStateChangeAction(IBiFunction<DocumentHeader, ChangeCommand, Task> action)
  {
    _afterStateChangeActions.Add(action);
  }

  private State Find(string desiredState)
  {
    return StateChangePredicates.Keys.Where(e => e.StateDescriptor.Equals(desiredState)).FirstMaybe()
      .OrElseDefault();
  }
}