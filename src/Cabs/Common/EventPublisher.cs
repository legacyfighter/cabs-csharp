using System.Threading.Channels;

namespace LegacyFighter.Cabs.Common;

public class EventsPublisher
{
  private readonly EventLoop _eventLoop;
  private readonly Channel<IEvent> _channel = Channel.CreateUnbounded<IEvent>();

  public EventsPublisher(EventLoop eventLoop)
  {
    _eventLoop = eventLoop;
  }

  public async Task Publish(IEvent @event)
  {
    await _channel.Writer.WriteAsync(@event);
  }

  public async Task Flush()
  {
    while (_channel.Reader.TryRead(out var @event))
    {
      await _eventLoop.Append(@event);
    }
  }
}
