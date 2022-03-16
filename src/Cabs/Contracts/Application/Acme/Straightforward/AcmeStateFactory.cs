using System.Reflection;
using LegacyFighter.Cabs.Contracts.Model;
using LegacyFighter.Cabs.Contracts.Model.State.Straightforward;
using LegacyFighter.Cabs.Contracts.Model.State.Straightforward.Acme;

namespace LegacyFighter.Cabs.Contracts.Application.Acme.Straightforward;

public class AcmeStateFactory
{
  public BaseState Create(DocumentHeader header)
  {
    //sample impl is based on class names
    //other possibilities: names Dependency Injection Containers, states persisted via ORM Discriminator mechanism, mapper
    var className = header.StateDescriptor;

    if (className == null)
    {
      var state = new DraftState();
      state.Init(header);
      return state;
    }
    else
    {
      var type = Assembly.GetAssembly(GetType()).DefinedTypes.Single(t => t.Name == className);
      var state = (BaseState)Activator.CreateInstance(type);
      state.Init(header);
      return state;
    }
  }
}