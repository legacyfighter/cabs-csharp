using System.Threading.Channels;
using MediatR;

namespace LegacyFighter.Cabs.Common;

//warning: this isn't a production-ready solution!
public class EventLoop : BackgroundService
{
  private readonly IServiceProvider _serviceProvider;
  private readonly ILogger<EventsPublisher> _logger;
  private readonly Channel<IEvent> _events = Channel.CreateUnbounded<IEvent>();

  public EventLoop(IServiceProvider serviceProvider, ILogger<EventsPublisher> logger)
  {
    _serviceProvider = serviceProvider;
    _logger = logger;
  }

  protected override async Task ExecuteAsync(CancellationToken stoppingToken)
  {
    while (!stoppingToken.IsCancellationRequested)
    {
      var @event = await _events.Reader.ReadAsync(stoppingToken);
      try
      {
        await using (_serviceProvider.CreateAsyncScope())
        {
          await _serviceProvider.GetRequiredService<IMediator>()
            .Publish(@event, stoppingToken);
        }
      }
      catch (Exception e)
      {
        _logger.LogError(e, "Could not publish event");
      }
    }
  }

  public async Task Append(IEvent @event)
  {
    await _events.Writer.WriteAsync(@event);
  }
}