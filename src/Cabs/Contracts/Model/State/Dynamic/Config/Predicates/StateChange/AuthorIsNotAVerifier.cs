using LegacyFighter.Cabs.Contracts.FunctionalInterfaces;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Actions;

namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Predicates.StateChange;

public class AuthorIsNotAVerifier : IBiFunction<State, ChangeCommand, bool>
{
  public const string ParamVerifier = ChangeVerifier.ParamVerifier;

  public bool Apply(State state, ChangeCommand command)
  {
    return !command.GetParam<long?>(ParamVerifier).Equals(state.DocumentHeader.AuthorId);
  }
}
