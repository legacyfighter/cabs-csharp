using LegacyFighter.Cabs.Repository;

namespace LegacyFighter.Cabs.Contracts.Legacy;

public interface IUserRepository
{
  Task<User> Find(long? authorId);
  Task<User> Save(User user);
}

public class EfCoreUserRepository : IUserRepository
{
  private readonly SqLiteDbContext _dbContext;

  public EfCoreUserRepository(SqLiteDbContext dbContext)
  {
    _dbContext = dbContext;
  }

  public async Task<User> Find(long? authorId)
  {
    return await _dbContext.Users.FindAsync(authorId);
  }

  public async Task<User> Save(User user)
  {
    _dbContext.Users.Update(user);
    await _dbContext.SaveChangesAsync();
    return user;
  }
}