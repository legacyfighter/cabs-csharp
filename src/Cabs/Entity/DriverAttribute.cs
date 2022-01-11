namespace LegacyFighter.Cabs.Entity;

public class DriverAttribute
{
  public enum DriverAttributeNames
  {
    PenaltyPoints,
    Nationality,
    YearsOfExperience,
    MedicalExaminationExpirationDate,
    MedicalExaminationRemarks,
    Email,
    Birthplace,
    CompanyName
  }

  public long Id { get; set; }

  protected DriverAttribute()
  {

  }

  public DriverAttribute(Driver driver, DriverAttributeNames attr, string value)
  {
    Driver = driver;
    Value = value;
    Name = attr;
  }

  public DriverAttributeNames Name { get; set; }
  public string Value { get; set; }
  public virtual Driver Driver { get; set; }
}