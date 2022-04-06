using System.Text.RegularExpressions;

namespace LegacyFighter.Cabs.DriverFleet;

public class DriverLicense
{
  internal const string DriverLicenseRegex = "^[A-Z9]{5}\\d{6}[A-Z9]{2}\\d[A-Z]{2}$";
  internal string ValueAsString { get; private set; }

  protected DriverLicense()
  {
  }

  private DriverLicense(string value)
  {
    ValueAsString = value;
  }

  internal static DriverLicense WithLicense(string driverLicense)
  {
    if (string.IsNullOrEmpty(driverLicense) || !Regex.IsMatch(driverLicense, DriverLicenseRegex))
    {
      throw new ArgumentException("Illegal license no = " + driverLicense);
    }

    return new DriverLicense(driverLicense);
  }

  internal static DriverLicense WithoutValidation(string driverLicense)
  {
    return new DriverLicense(driverLicense);
  }

  public override string ToString()
  {
    return "DriverLicense{" +
           "driverLicense='" + ValueAsString + '\'' +
           '}';
  }
}