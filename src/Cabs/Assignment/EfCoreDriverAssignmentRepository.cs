using LegacyFighter.Cabs.Repository;
using Microsoft.EntityFrameworkCore;

namespace LegacyFighter.Cabs.Assignment;

public interface IDriverAssignmentRepository
{
  Task<DriverAssignment> FindByRequestId(Guid requestId);
  Task<DriverAssignment> FindByRequestIdAndStatus(Guid requestId, AssignmentStatuses status);
  Task Save(DriverAssignment driverAssignment);
}

public class EfCoreDriverAssignmentRepository : IDriverAssignmentRepository
{
  private readonly SqLiteDbContext _dbContext;

  public EfCoreDriverAssignmentRepository(SqLiteDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<DriverAssignment> FindByRequestId(Guid requestId)
  {
    return await _dbContext.DriverAssignments.FirstOrDefaultAsync(a => a.RequestId == requestId);
  }

  public async Task<DriverAssignment> FindByRequestIdAndStatus(Guid requestId, AssignmentStatuses status)
  {
    return await _dbContext.DriverAssignments.FirstOrDefaultAsync(
      a => a.RequestId == requestId && a.Status == status);
  }

  public async Task Save(DriverAssignment driverAssignment)
  {
    _dbContext.DriverAssignments.Update(driverAssignment);
    await _dbContext.SaveChangesAsync();
  }
}