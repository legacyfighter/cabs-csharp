using LegacyFighter.Cabs.Common;
using LegacyFighter.Cabs.Repair.Legacy.Job;

namespace LegacyFighter.Cabs.Repair.Legacy.User;

public abstract class CommonBaseAbstractUser : BaseEntity
{
  public JobResult DoJob(CommonBaseAbstractJob job)
  {
    //poor man's pattern matching
    if (job is RepairJob)
    {
      return Handle((RepairJob)job);
    }

    if (job is MaintenanceJob)
    {
      return Handle((MaintenanceJob)job);
    }

    return DefaultHandler(job);
  }

  protected virtual JobResult Handle(RepairJob job)
  {
    return DefaultHandler(job);
  }

  protected virtual JobResult Handle(MaintenanceJob job)
  {
    return DefaultHandler(job);
  }

  protected JobResult DefaultHandler(CommonBaseAbstractJob job)
  {
    throw new ArgumentException(GetType().Name + " can not handle " + job.GetType().Name);
  }
}