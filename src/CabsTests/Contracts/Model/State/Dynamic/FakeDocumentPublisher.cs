using System.Collections.Generic;
using System.Linq;
using LegacyFighter.Cabs.Contracts.Application;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Events;

namespace LegacyFighter.CabsTests.Contracts.Model.State.Dynamic;

public class FakeDocumentPublisher : IApplicationEventPublisher
{
  private readonly ISet<object> _events = new HashSet<object>();

  public async Task Publish(DocumentEvent @event)
  {
    _events.Add(@event);
  }

  public void Contains<TEvent>() where TEvent : DocumentEvent
  {
    var found = _events.Any(e => e.GetType() == typeof(TEvent));
    Assert.True(found);
  }

  public void NoEvents()
  {
    Assert.AreEqual(0, _events.Count);
  }

  public void Reset()
  {
    _events.Clear();
  }
}