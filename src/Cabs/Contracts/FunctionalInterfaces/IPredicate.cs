namespace LegacyFighter.Cabs.Contracts.FunctionalInterfaces;

public interface IPredicate<in T>
{
  bool Test(T state);
}