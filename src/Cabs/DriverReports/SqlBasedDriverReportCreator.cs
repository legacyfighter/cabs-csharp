using System.Data;
using System.Data.Common;
using LegacyFighter.Cabs.DistanceValue;
using LegacyFighter.Cabs.Dto;
using LegacyFighter.Cabs.Entity;
using LegacyFighter.Cabs.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;
using NodaTime.TimeZones;

namespace LegacyFighter.Cabs.DriverReports;

public class SqlBasedDriverReportCreator : IDriverReportCreator
{
  private static readonly string QueryForDriverWithAttrs =
    "SELECT d.id, d.FirstName, d.LastName, d.DriverLicense, " +
    "d.Photo, d.Status, d.Type, attr.Name, attr.Value " +
    "FROM Drivers d " +
    "LEFT JOIN DriverAttributes attr ON d.id = attr.DriverId " +
    "WHERE d.id = :driverId AND attr.name <> :filteredAttr";

  private const string QueryForSessions =
    "SELECT ds.LoggedAt, ds.LoggedOutAt, ds.PlatesNumber, ds.CarClass, ds.CarBrand, " +
    "td.TransitId as TransitId, td.Name as TariffName, td.Status as TransitStatus, td.Km, td.KmRate, " +
    "td.Price, td.DriversFee, td.EstimatedPrice, td.BaseFee, " +
    "td.DateTime, td.PublishedAt, td.AcceptedAt, td.Started, td.CompleteAt, td.CarType, " +
    "cl.Id as ClaimId, cl.OwnerId, cl.Reason, cl.IncidentDescription, cl.Status as ClaimStatus, cl.CreationDate, " +
    "cl.CompletionDate, cl.ChangeDate, cl.CompletionMode, cl.ClaimNo, " +
    "af.Country as AfCountry, af.City as AfCity, af.Street AS AfStreet, af.BuildingNumber AS AfNumber, " +
    "ato.Country as AtoCountry, ato.City as AtoCity, ato.Street AS AtoStreet, ato.BuildingNumber AS AtoNumber " +
    "FROM DriverSessions ds " +
    "LEFT JOIN TransitsDetails td ON ds.DriverId = td.DriverId " +
    "LEFT JOIN Addresses af ON td.FromId = af.Id " +
    "LEFT JOIN Addresses ato ON td.ToId = ato.Id " +
    "LEFT JOIN Claims cl ON cl.TransitId = td.TransitId " +
    "WHERE ds.DriverId = :driverId AND td.Status = :transitStatus " +
    "AND ds.LoggedAt >= :since " +
    "AND td.CompleteAt >= ds.LoggedAt " +
    "AND td.CompleteAt <= ds.LoggedOutAt GROUP BY ds.Id";

  private readonly SqLiteDbContext _dbContext;
  private readonly IClock _clock;

  public SqlBasedDriverReportCreator(SqLiteDbContext dbContext, IClock clock)
  {
    _dbContext = dbContext;
    _clock = clock;
  }

  public async Task<DriverReport> CreateReport(long? driverId, int lastDays)
  {
    var driverReport = new DriverReport();

    var queryForDriversWithAttrs = _dbContext.Database.GetDbConnection().CreateCommand();
    queryForDriversWithAttrs.CommandText = QueryForDriverWithAttrs;
    queryForDriversWithAttrs.Parameters.Add(new SqliteParameter("driverId", driverId));
    queryForDriversWithAttrs.Parameters.Add(
      new SqliteParameter("filteredAttr", DriverAttribute.DriverAttributeNames.MedicalExaminationRemarks.ToString()));
    var driverInfo = await Execute(queryForDriversWithAttrs);

    foreach (var record in driverInfo)
    {
      AddAttrToReport(driverReport, record);
    }

    if (driverInfo.Any())
    {
      AddDriverToReport(driverReport, driverInfo.First());
    }

    var queryForSessions = _dbContext.Database.GetDbConnection().CreateCommand();
    queryForSessions.CommandText = QueryForSessions;
    queryForSessions.Parameters.Add(new SqliteParameter("driverId", driverId));
    queryForSessions.Parameters.Add(new SqliteParameter("transitStatus", (int)Transit.Statuses.Completed));
    queryForSessions.Parameters.Add(new SqliteParameter("since", CalculateStartingPoint(lastDays).ToUnixTimeTicks()));
    var resultEnumerable = await Execute(queryForSessions);

    var sessions = new Dictionary<DriverSessionDto, List<TransitDto>>();
    foreach (var dataRow in resultEnumerable)
    {
      var key = RetrieveDrivingSession(dataRow);
      var value = new List<TransitDto> { RetrieveTransit(dataRow) };
      if (sessions.ContainsKey(key))
      {
        sessions[key].AddRange(value);
      }
      else
      {
        sessions[key] = value;
      }
    }

    driverReport.Sessions = sessions;
    return driverReport;
  }

  private static async Task<IReadOnlyList<DataRow>> Execute(DbCommand dbCommand)
  {
    await using var reader = await dbCommand.ExecuteReaderAsync();
    var rows = new List<DataRow>();
    var dataTable = new DataTable();
    for (var i = 0; i < reader.FieldCount; i++)
    {
      dataTable.Columns.Add(new DataColumn(reader.GetName(i)));
    }
    while (await reader.ReadAsync())
    {
      var row = dataTable.NewRow();
      for (var i = 0; i < reader.FieldCount; i++)
      {
        row[reader.GetName(i)] = reader.GetValue(i);
      }
      rows.Add(row);
    }

    return rows;
  }

  private TransitDto RetrieveTransit(DataRow row)
  {
    return new TransitDto(
      row["TransitId"].ToNullableLong(),
      row["TariffName"].ToString(),
      Enum.Parse<Transit.Statuses>(row["TransitStatus"].ToString()),
      null,
      Distance.OfKm(row["Km"].ToFloat()),
      row["KmRate"].ToFloat(),
      new decimal(row["Price"].ToInt()),
      new decimal(row["DriversFee"].ToInt()),
      new decimal(row["EstimatedPrice"].ToInt()),
      new decimal(row["BaseFee"].ToInt()),
      row["DateTime"].ToNullableInstant(),
      row["PublishedAt"].ToNullableInstant(),
      row["AcceptedAt"].ToNullableInstant(),
      row["Started"].ToNullableInstant(),
      row["CompleteAt"].ToNullableInstant(),
      RetrieveClaim(row),
      null,
      RetrieveFromAddress(row),
      RetrieveToAddress(row),
      Enum.Parse<CarType.CarClasses>(row["CarType"].ToString()),
      null);
  }

  private DriverSessionDto RetrieveDrivingSession(DataRow row)
  {
    return new DriverSessionDto(
      row["LoggedAt"].ToInstant(),
      row["LoggedOutAt"].ToNullableInstant(),
      row["PlatesNumber"].ToString(),
      Enum.Parse<CarType.CarClasses>(row["CarClass"].ToString()),
      row["CarBrand"].ToString());
  }

  private AddressDto RetrieveToAddress(DataRow row)
  {
    return new AddressDto((string)row["AfCountry"],
      (string)row["AfCity"],
      (string)row["AfStreet"],
      row.IsNull("AfNumber") ? null : row["AfNumber"].ToInt());
  }

  private AddressDto RetrieveFromAddress(DataRow row)
  {
    return new AddressDto(row["AfCountry"].ToString(),
      row["AfCity"].ToString(),
      row["AfStreet"].ToString(),
      row.IsNull("AfNumber") ? null : row["AfNumber"].ToInt());
  }

  private ClaimDto RetrieveClaim(DataRow row)
  {
    if (row.IsNull("ClaimId"))
    {
      return null;
    }
    var claimId = row["ClaimId"].ToLong();

    return new ClaimDto(
      claimId,
      row["OwnerId"].ToNullableLong(),
      row["TransitId"].ToNullableLong(),
      row["Reason"].ToString(),
      row["IncidentDescription"].ToString(),
      row["CreationDate"].ToInstant(),
      row.IsNull("CompletionDate") ? null : row["CompletionDate"].ToInstant(),
      row.IsNull("ChangeDate") ? null : row["ChangeDate"].ToInstant(),
      row.IsNull("CompletionMode") ? null : Enum.Parse<Claim.CompletionModes>(row["CompletionMode"].ToString()),
      Enum.Parse<Claim.Statuses>(row["ClaimStatus"].ToString()),
      row["ClaimNo"].ToString());
  }

  private Instant CalculateStartingPoint(int lastDays)
  {
    var beggingOfToday = _clock.GetCurrentInstant().InZone(BclDateTimeZone.ForSystemDefault()).LocalDateTime.Date
      .AtStartOfDayInZone(DateTimeZone.Utc).ToInstant();
    var since = beggingOfToday.Minus(Duration.FromDays(lastDays));
    return since;
  }

  private void AddDriverToReport(DriverReport driverReport, DataRow row)
  {
    driverReport.DriverDto = new DriverDto(
      row["Id"].ToNullableLong(),
      row["FirstName"].ToString(),
      row["LastName"].ToString(),
      row["DriverLicense"].ToString(),
      row["Photo"].ToString(),
      Enum.Parse<Driver.Statuses>(row["Status"].ToString()),
      row.IsNull("Type") ? null : Enum.Parse<Driver.Types>(row["Type"].ToString()));
  }

  private void AddAttrToReport(DriverReport driverReport, DataRow row)
  {
    driverReport.AddAttr(
      Enum.Parse<DriverAttribute.DriverAttributeNames>((string)row["NAME"]), (string)row[8]);
  }
}

public static class RowElementConversions
{
  private delegate bool TryParse<TOut>(string input, out TOut? output);

  public static int ToInt(this object obj)
  {
    return int.Parse(obj.ToString());
  }
  
  public static long ToLong(this object obj)
  {
    return long.Parse(obj.ToString());
  }
  
  public static float ToFloat(this object obj)
  {
    return float.Parse(obj.ToString());
  }

  public static long? ToNullableLong(this object? obj)
  {
    return obj.ConvertToNullable<long, long>(long.TryParse, n => n);
  }

  public static Instant ToInstant(this object obj)
  {
    return Instant.FromUnixTimeTicks(long.Parse(obj.ToString()));
  }

  public static Instant? ToNullableInstant(this object? obj)
  {
    return obj.ConvertToNullable<long, Instant>(
      long.TryParse, 
      Instant.FromUnixTimeTicks);
  }

  private static TReturn? ConvertToNullable<TParsed, TReturn>(
    this object? obj, 
    TryParse<TParsed> tryParse, 
    Func<TParsed, TReturn> convert)
  {
    if (obj is null or DBNull)
    {
      return default;
    }

    if (tryParse(obj.ToString(), out var parsed))
    {
      return convert(parsed);
    }

    throw new InvalidOperationException(obj.ToString());
  }
  

}