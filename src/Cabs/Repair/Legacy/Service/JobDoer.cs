using LegacyFighter.Cabs.Repair.Legacy.Dao;
using LegacyFighter.Cabs.Repair.Legacy.Job;

namespace LegacyFighter.Cabs.Repair.Legacy.Service;

public class JobDoer : IJobDoer
{
  private readonly UserDao _userDao;

  public JobDoer(UserDao userDao)
  {
    _userDao = userDao; //I'll inject test double some day because it makes total sense to me
  }

  public async Task<JobResult> Repair(long? userId, CommonBaseAbstractJob job)
  {
    var user = await _userDao.FindBy(userId);
    return user.DoJob(job);
  }
}