using LegacyFighter.Cabs.Contracts.FunctionalInterfaces;

namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Actions;

public class ChangeVerifier : IBiFunction<DocumentHeader, ChangeCommand, Task>
{
  public const string ParamVerifier = "verifier";

  public async Task Apply(DocumentHeader documentHeader, ChangeCommand command)
  {
    documentHeader.VerifierId = command.GetParam<long?>(ParamVerifier);
  }
}