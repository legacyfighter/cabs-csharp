using System.IO;
using DotNet.Testcontainers.Containers.Builders;
using DotNet.Testcontainers.Containers.Modules;
using DotNet.Testcontainers.Containers.OutputConsumers;
using DotNet.Testcontainers.Containers.WaitStrategies;

namespace LegacyFighter.CabsTests.Common;

public class TestWithGraphDb
{
  private TestcontainersContainer _neo4J = default!;
  private NUnitConsumer _outputConsumer = default!;
  private const int InternalHttpPort = 7474;
  private const int InternalBoltPort = 7687;
  protected string Neo4JBoltUri => $"neo4j://{_neo4J.Hostname}:{_neo4J.GetMappedPublicPort(InternalBoltPort)}";

  [SetUp]
  public async Task SetUp()
  {
    _outputConsumer = new NUnitConsumer();
    _neo4J = new TestcontainersBuilder<TestcontainersContainer>()
      .WithImage("neo4j:4.4.4")
      .WithPortBinding(0, InternalHttpPort)
      .WithPortBinding(0, InternalBoltPort)
      .WithEnvironment("NEO4J_AUTH", "none")
      .WithCleanUp(true)
      .WithWaitStrategy(Wait.ForUnixContainer()
        .UntilPortIsAvailable(InternalHttpPort)
        .UntilPortIsAvailable(InternalBoltPort))
      .WithOutputConsumer(_outputConsumer)
      .Build();
    await _neo4J.StartAsync();
  }

  [TearDown]
  public async Task TearDown()
  {
    _outputConsumer.Dispose();
    await _neo4J.DisposeAsync();
  }

  private class NUnitConsumer : IOutputConsumer
  {
    private readonly MemoryStream _stream = new();
  
    public NUnitConsumer()
    {
      Stderr = _stream;
      Stdout = _stream;
    }

    public void Dispose()
    {
      _stream.Position = 0;
      using var reader = new StreamReader(_stream);
      var logs = reader.ReadToEnd();
      TestContext.Out.WriteLine(logs);
      _stream.Close();
    }

    public Stream Stdout { get; }
    public Stream Stderr { get; }
  }

}