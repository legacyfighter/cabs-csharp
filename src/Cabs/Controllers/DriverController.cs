using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using LegacyFighter.Cabs.Service;
using Microsoft.AspNetCore.Mvc;

namespace LegacyFighter.Cabs.Controllers;

[ApiController]
[Route("[controller]")]
public class DriverController {

  private readonly IDriverService _driverService;
  private IDriverRepository _driverRepository;

  public DriverController(IDriverService driverService, IDriverRepository driverRepository)
  {
    _driverService = driverService;
    _driverRepository = driverRepository;
  }

  [HttpPost("/drivers")]
  public async Task<DriverDto> CreateDriver([FromQuery] string license, [FromQuery] string firstName, [FromQuery] string lastName, [FromQuery] string photo) 
  {
    var driver = await _driverService.CreateDriver(license, lastName, firstName, Driver.Types.Candidate, Driver.Statuses.Inactive, photo);

    return await _driverService.LoadDriver(driver.Id);
  }

  [HttpGet("/drivers/{id}")]
  public async Task<DriverDto> GetDriver(long id) 
  {
    return await _driverService.LoadDriver(id);
  }

  [HttpPost("/drivers/{id}")]
  public async Task<DriverDto> UpdateDriver(long id) 
  {

    return await _driverService.LoadDriver(id);
  }

  [HttpPost("/drivers/{id}/deactivate")]
  public async Task<DriverDto> DeactivateDriver(long id) 
  {
    await _driverService.ChangeDriverStatus(id, Driver.Statuses.Inactive);

    return await _driverService.LoadDriver(id);
  }

  [HttpPost("/drivers/{id}/activate")]
  public async Task<DriverDto> ActivateDriver(long id) 
  {
    await _driverService.ChangeDriverStatus(id, Driver.Statuses.Active);

    return await _driverService.LoadDriver(id);
  }
}