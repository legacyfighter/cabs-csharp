using LegacyFighter.Cabs.Contracts.Model.State.Dynamic.Config.Events;
using MediatR;

namespace LegacyFighter.Cabs.Contracts.Application;

public interface IApplicationEventPublisher 
{
  Task Publish(DocumentEvent @event);
}

class MediatRApplicationEventPublisher : IApplicationEventPublisher
{
  private readonly IMediator _mediator;

  public MediatRApplicationEventPublisher(IMediator mediator)
  {
    _mediator = mediator;
  }

  public async Task Publish(DocumentEvent @event)
  {
    await _mediator.Publish(@event);
  }
}