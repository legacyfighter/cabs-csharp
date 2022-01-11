using LegacyFighter.Cabs.Entity;

namespace LegacyFighter.Cabs.Dto;

public class DriverDto
{
  public DriverDto(Driver driver)
  {
    Id = driver.Id;
    FirstName = driver.FirstName;
    LastName = driver.LastName;
    DriverLicense = driver.DriverLicense;
    Photo = driver.Photo;
    Status = driver.Status;
    Type = driver.Type;
  }

  public long? Id { get; set; }
  public string FirstName { get; set; }
  public string LastName { get; set; }
  public string DriverLicense { get; set; }
  public string Photo { get; set; }
  public Driver.Statuses Status { get; set; }
  public Driver.Types? Type { get; set; }
}