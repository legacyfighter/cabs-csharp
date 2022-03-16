namespace LegacyFighter.Cabs.Repair.Legacy.Job;

public class JobResult
{
  public enum Decisions
  {
    Redirection,
    Accepted,
    Error
  }

  private readonly Dictionary<string, object> _params = new();

  public JobResult(Decisions decision)
  {
    Decision = decision;
  }

  public Decisions Decision { get; }

  public JobResult AddParam(string name, object value)
  {
    _params[name] = value;
    return this;
  }

  public object GetParam(string name)
  {
    return _params[name];
  }

}