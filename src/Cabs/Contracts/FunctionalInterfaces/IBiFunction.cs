namespace LegacyFighter.Cabs.Contracts.FunctionalInterfaces;

public interface IBiFunction<in T, in T1, out TReturn>
{
  public TReturn Apply(T arg1, T1 arg2);
}