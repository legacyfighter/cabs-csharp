namespace LegacyFighter.Cabs.Contracts.Legacy;

public interface IVersionable
{
  void RecreateTo(long version);
  long GetLastVersion();
}