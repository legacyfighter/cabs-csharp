namespace LegacyFighter.Cabs.Common;

public interface ITransaction : IAsyncDisposable
{
  Task Commit();
}