namespace LegacyFighter.Cabs.Contracts.FunctionalInterfaces;

public interface IFunction<in T, out TReturn>
{
  TReturn Apply(T state);
}