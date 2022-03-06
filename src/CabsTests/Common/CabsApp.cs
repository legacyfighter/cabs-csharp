using System;
using System.Collections.Generic;
using LegacyFighter.Cabs.Controllers;
using LegacyFighter.Cabs.DriverReports;
using LegacyFighter.Cabs.DriverReports.TravelledDistances;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using LegacyFighter.Cabs.TransitAnalyzer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace LegacyFighter.CabsTests.Common;

internal class CabsApp : WebApplicationFactory<Program>
{
  private IServiceScope _scope;
  private readonly Action<IServiceCollection> _customization;
  
  /// <summary>
  /// https://stackoverflow.com/questions/66942392/unwanted-unique-constraint-in-many-to-many-relationship
  /// </summary>
  private bool _reuseScope = false;
  private readonly Dictionary<string, string> _configurationOverrides;

  private CabsApp(Action<IServiceCollection> customization, Dictionary<string, string> configurationOverrides)
  {
    _customization = customization;
    _configurationOverrides = configurationOverrides;
    _scope = base.Services.CreateAsyncScope();
  }

  public static CabsApp CreateInstance()
  {
    var cabsApp = new CabsApp(_ => { }, new Dictionary<string, string>());
    return cabsApp;
  }

  public static CabsApp CreateInstance(Dictionary<string, string> configurationOverrides)
  {
    var cabsApp = new CabsApp(_ => { }, configurationOverrides);
    return cabsApp;
  }

  public static CabsApp CreateInstance(Action<IServiceCollection> customization)
  {
    var cabsApp = new CabsApp(customization, new Dictionary<string, string>());
    return cabsApp;
  }

  public static CabsApp CreateInstance(Action<IServiceCollection> customization, Dictionary<string, string> configurationOverrides)
  {
    var cabsApp = new CabsApp(customization, configurationOverrides);
    return cabsApp;
  }

  protected override void ConfigureWebHost(IWebHostBuilder builder)
  {
    base.ConfigureWebHost(builder);
    builder.ConfigureAppConfiguration(configurationBuilder =>
    {
      configurationBuilder.AddInMemoryCollection(_configurationOverrides);
    });
    builder.ConfigureServices(collection => collection.AddTransient<Fixtures>());
    builder.ConfigureServices(_customization);
  }

  public void StartReuseRequestScope()
  {
    _reuseScope = true;
  }

  public void EndReuseRequestScope()
  {
    _reuseScope = false;
  }

  protected override void Dispose(bool disposing)
  {
    _scope?.Dispose();
    base.Dispose(disposing);
  }

  private IServiceScope RequestScope()
  {
    if (!_reuseScope)
    {
      _scope.Dispose();
      _scope = Services.CreateAsyncScope();
    }
    return _scope;
  }

  public Fixtures Fixtures 
    => RequestScope().ServiceProvider.GetRequiredService<Fixtures>();

  public IDriverFeeService DriverFeeService
    => RequestScope().ServiceProvider.GetRequiredService<IDriverFeeService>();

  public IDriverService DriverService
    => RequestScope().ServiceProvider.GetRequiredService<IDriverService>();

  public ITransitService TransitService
    => RequestScope().ServiceProvider.GetRequiredService<ITransitService>();

  public IDriverSessionService DriverSessionService
    => RequestScope().ServiceProvider.GetRequiredService<IDriverSessionService>();

  public IDriverTrackingService DriverTrackingService
    => RequestScope().ServiceProvider.GetRequiredService<IDriverTrackingService>();

  public TransitController TransitController
    => RequestScope().ServiceProvider.GetRequiredService<TransitController>();

  public ICarTypeService CarTypeService
    => RequestScope().ServiceProvider.GetRequiredService<ICarTypeService>();

  public IClaimService ClaimService
    => RequestScope().ServiceProvider.GetRequiredService<IClaimService>();

  public IAwardsService AwardsService
    => RequestScope().ServiceProvider.GetRequiredService<IAwardsService>();

  public IAwardsAccountRepository AwardsAccountRepository
    => RequestScope().ServiceProvider.GetRequiredService<IAwardsAccountRepository>();

  public IContractService ContractService
    => RequestScope().ServiceProvider.GetRequiredService<IContractService>();

  public DriverReportController DriverReportController 
    => RequestScope().ServiceProvider.GetRequiredService<DriverReportController>();

  public IAddressRepository AddressRepository
    => RequestScope().ServiceProvider.GetRequiredService<IAddressRepository>();

  public ITravelledDistanceService TravelledDistanceService
    => RequestScope().ServiceProvider.GetRequiredService<ITravelledDistanceService>();

  public TransitAnalyzerController TransitAnalyzerController
    => RequestScope().ServiceProvider.GetRequiredService<TransitAnalyzerController>();

  public GraphTransitAnalyzer GraphTransitAnalyzer
    => RequestScope().ServiceProvider.GetRequiredService<GraphTransitAnalyzer>();
}