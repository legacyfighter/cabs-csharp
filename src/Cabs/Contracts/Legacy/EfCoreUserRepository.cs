using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.Contracts.Legacy;

public interface IUserRepository
{
}

public class EfCoreUserRepository : IUserRepository
{
  private readonly SqLiteDbContext _dbContext;

  public EfCoreUserRepository(SqLiteDbContext dbContext)
  {
    _dbContext = dbContext;
  }
}