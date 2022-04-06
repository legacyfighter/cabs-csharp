using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Geolocation.Address;

namespace LegacyFighter.Cabs.Geolocation;

public static class GeolocationDependencies
{
  public static void AddTo(WebApplicationBuilder builder)
  {
    builder.Services.AddTransient<IAddressRepositoryInterface, EfCoreAddressRepository>();
    builder.Services.AddTransient<IGeocodingService, GeocodingService>();
    builder.Services.AddTransient<DistanceCalculator>();
    builder.Services.AddTransient<AddressRepository>();
    builder.Services.AddTransient<IAddressRepository>(ctx => 
      new TransactionalAddressRepository(
        ctx.GetRequiredService<AddressRepository>(),
        ctx.GetRequiredService<ITransactions>()));
  }
}