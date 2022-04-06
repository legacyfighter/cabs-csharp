using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Assignment;

public interface IDriverAssignmentRepository
{
  Task<DriverAssignment> FindByRequestGuid(Guid requestGuid);
  Task<DriverAssignment> FindByRequestGuidAndStatus(Guid requestGuid, AssignmentStatuses status);
  Task Save(DriverAssignment driverAssignment);
}

public class EfCoreDriverAssignmentRepository : IDriverAssignmentRepository
{
  private readonly SqLiteDbContext _dbContext;

  public EfCoreDriverAssignmentRepository(SqLiteDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<DriverAssignment> FindByRequestGuid(Guid requestGuid)
  {
    return await _dbContext.DriverAssignments.FirstOrDefaultAsync(a => a.RequestGuid == requestGuid);
  }

  public async Task<DriverAssignment> FindByRequestGuidAndStatus(Guid requestGuid, AssignmentStatuses status)
  {
    return await _dbContext.DriverAssignments.FirstOrDefaultAsync(
      a => a.RequestGuid == requestGuid && a.Status == status);
  }

  public async Task Save(DriverAssignment driverAssignment)
  {
    _dbContext.DriverAssignments.Update(driverAssignment);
    await _dbContext.SaveChangesAsync();
  }
}