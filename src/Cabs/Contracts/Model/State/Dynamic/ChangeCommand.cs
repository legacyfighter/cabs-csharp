namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic;

public class ChangeCommand
{
  private Dictionary<string, object> _params;

  public ChangeCommand(string desiredState, Dictionary<string, object> @params)
  {
    DesiredState = desiredState;
    _params = @params;
  }

  public ChangeCommand(string desiredState) : this(desiredState, new Dictionary<string, object>())
  {
  }

  public ChangeCommand WithParam(string name, object value)
  {
    _params[name] = value;
    return this;
  }

  public string DesiredState { get; }

  public T GetParam<T>(string name)
  {
    return (T)_params[name];
  }

  public override string ToString()
  {
    return "ChangeCommand{" +
           "desiredState='" + DesiredState + '\'' +
           '}';
  }
}