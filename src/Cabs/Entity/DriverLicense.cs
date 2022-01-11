using System.Text.RegularExpressions;

namespace LegacyFighter.Cabs.Entity;

public class DriverLicense
{
  public const string DriverLicenseRegex = "^[A-Z9]{5}\\d{6}[A-Z9]{2}\\d[A-Z]{2}$";

  private readonly string _driverLicense;

  private DriverLicense(string driverLicense)
  {
    _driverLicense = driverLicense;
  }

  public static DriverLicense WithLicense(string driverLicense)
  {
    if (string.IsNullOrEmpty(driverLicense) || !Regex.IsMatch(driverLicense, DriverLicenseRegex))
    {
      throw new ArgumentException("Illegal license no = " + driverLicense);
    }

    return new DriverLicense(driverLicense);
  }

  public static DriverLicense WithoutValidation(string driverLicense)
  {
    return new DriverLicense(driverLicense);
  }

  public override string ToString()
  {
    return "DriverLicense{" +
           "driverLicense='" + _driverLicense + '\'' +
           '}';
  }

  public string AsString()
  {
    return _driverLicense;
  }
}