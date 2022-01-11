using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LegacyFighter.CabsTests.Common;

internal class CabsApp : WebApplicationFactory<Program>
{
  private IServiceScope _scope;

  private CabsApp()
  {
    _scope = base.Services.CreateAsyncScope();
  }

  public static CabsApp CreateInstance()
  {
    var cabsApp = new CabsApp();
    return cabsApp;
  }

  protected override void Dispose(bool disposing)
  {
    _scope.Dispose();
    base.Dispose(disposing);
  }

  private IServiceScope NewRequestScope()
  {
    _scope.Dispose();
    _scope = Services.CreateAsyncScope();
    return _scope;
  }

  public AddressRepository AddressRepository 
    => NewRequestScope().ServiceProvider.GetRequiredService<AddressRepository>();

  public ITransitRepository TransitRepository
    => NewRequestScope().ServiceProvider.GetRequiredService<ITransitRepository>();

  public IDriverFeeService DriverFeeService
    => NewRequestScope().ServiceProvider.GetRequiredService<IDriverFeeService>();

  public IDriverService DriverService
    => NewRequestScope().ServiceProvider.GetRequiredService<IDriverService>();

  public IClientRepository ClientRepository
    => NewRequestScope().ServiceProvider.GetRequiredService<IClientRepository>();

  public IDriverFeeRepository DriverFeeRepository
    => NewRequestScope().ServiceProvider.GetRequiredService<IDriverFeeRepository>();
}