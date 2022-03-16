using LegacyFighter.Cabs.Contracts.Application;
using LegacyFighter.Cabs.Contracts.FunctionalInterfaces;
using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Events;

namespace LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Actions;

public class PublishEvent<T> : IBiFunction<DocumentHeader, ChangeCommand, Task> where T : DocumentEvent
{
  private readonly IApplicationEventPublisher _publisher;

  public PublishEvent(IApplicationEventPublisher publisher)
  {
    _publisher = publisher;
  }

  public async Task Apply(DocumentHeader documentHeader, ChangeCommand command)
  {
    var @event = (DocumentEvent)Activator.CreateInstance(typeof(T), 
      documentHeader.Id, 
      documentHeader.StateDescriptor, 
      documentHeader.ContentId, 
      documentHeader.DocumentNumber);

    await _publisher.Publish(@event);
  }
}