using LegacyFighter.Cabs.Repair.Legacy.Job;

namespace LegacyFighter.Cabs.Repair.Legacy.Service;

public interface IJobDoer
{
  Task<JobResult> Repair(long? userId, CommonBaseAbstractJob job);
}