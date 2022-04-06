namespace LegacyFighter.Cabs.DriverFleet;

public class DriverDto
{
  public DriverDto(
    long? id,
    string firstName,
    string lastName,
    string driverLicense,
    string photo,
    Driver.Statuses status,
    Driver.Types? type) 
  {
    Id = id;
    FirstName = firstName;
    LastName = lastName;
    DriverLicense = driverLicense;
    Photo = photo;
    Status = status;
    Type = type;
  }

  public DriverDto(Driver driver)
  {
    Id = driver.Id;
    FirstName = driver.FirstName;
    LastName = driver.LastName;
    DriverLicense = driver.DriverLicense.ValueAsString;
    Photo = driver.Photo;
    Status = driver.Status;
    Type = driver.Type;
    IsOccupied = driver.Occupied;
  }

  public long? Id { get; set; }
  public string FirstName { get; set; }
  public string LastName { get; set; }
  public string DriverLicense { get; set; }
  public string Photo { get; set; }
  public Driver.Statuses Status { get; set; }
  public Driver.Types? Type { get; set; }
  public bool IsOccupied { get; private set; }
}