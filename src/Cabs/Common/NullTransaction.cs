namespace LegacyFighter.Cabs.Common;

public class NullTransaction : ITransaction
{
  public async ValueTask DisposeAsync()
  {
      
  }

  public async Task Commit()
  {

  }
}