using LegacyFighter.Cabs.Repository;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using NodaTime;

namespace LegacyFighter.Cabs.DriverFleet.DriverReports.TravelledDistances;

public interface ITravelledDistanceRepository
{
  Task<TravelledDistance> FindTravelledDistanceTimeSlotByTime(Instant when, long? driverId);
  Task<TravelledDistance?> FindTravelledDistanceByTimeSlotAndDriverId(TimeSlot timeSlot, long driverId);
  Task<double> CalculateDistance(Instant beginning, Instant to, long driverId);
  Task Save(TravelledDistance travelledDistance);
}

internal class EfCoreTravelledDistanceRepository : ITravelledDistanceRepository
{
  private readonly SqLiteDbContext _dbContext;

  public EfCoreTravelledDistanceRepository(SqLiteDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public Task<TravelledDistance> FindTravelledDistanceTimeSlotByTime(Instant when, long? driverId)
  {
    return _dbContext.TravelledDistances.FromSqlInterpolated(
        $"select * from TravelledDistances td where td.Beginning <= {when.ToUnixTimeTicks()} and {when.ToUnixTimeTicks()} < td.End and td.DriverId = {driverId}")
      .SingleOrDefaultAsync();
  }

  public Task<TravelledDistance?> FindTravelledDistanceByTimeSlotAndDriverId(TimeSlot timeSlot, long driverId)
  {
    return _dbContext.TravelledDistances.FromSqlInterpolated(
        $"select * from TravelledDistances td where td.Beginning = {timeSlot.Beginning.ToUnixTimeTicks()} and td.End = {timeSlot.End.ToUnixTimeTicks()} and td.DriverId = {driverId}")
      .SingleOrDefaultAsync();
  }

  public async Task<double> CalculateDistance(Instant beginning, Instant to, long driverId)
  {
    await using var dbCommand = _dbContext.Database.GetDbConnection().CreateCommand();

    dbCommand.CommandText = "SELECT COALESCE(SUM(_inner.Km), 0) FROM " +
                            "( (SELECT * FROM TravelledDistances td WHERE td.Beginning >= :beginning " +
                            "AND td.DriverId = :driverId)) " +
                            "AS _inner WHERE end <= :to ";
    dbCommand.Parameters.Add(new SqliteParameter("beginning", beginning.ToUnixTimeTicks()));
    dbCommand.Parameters.Add(new SqliteParameter("to", to.ToUnixTimeTicks()));
    dbCommand.Parameters.Add(new SqliteParameter("driverId", driverId));

    var dbResult = await dbCommand.ExecuteScalarAsync();
    return double.Parse(dbResult.ToString());
  }

  public async Task Save(TravelledDistance travelledDistance)
  {
    if (_dbContext.Entry(travelledDistance).State == EntityState.Detached)
    {
      await _dbContext.AddAsync(travelledDistance);
    }
    else
    {
      _dbContext.Update(travelledDistance);
    }

    await _dbContext.SaveChangesAsync();
  }
}