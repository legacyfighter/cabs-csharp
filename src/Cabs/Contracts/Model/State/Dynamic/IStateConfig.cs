namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic;

public interface IStateConfig
{
  State Begin(DocumentHeader documentHeader);
  State Recreate(DocumentHeader documentHeader);
}