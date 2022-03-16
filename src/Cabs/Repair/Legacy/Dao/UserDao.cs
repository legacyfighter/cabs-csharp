using LegacyFighter.Cabs.Repair.Legacy.Parts;
using LegacyFighter.Cabs.Repair.Legacy.User;

namespace LegacyFighter.Cabs.Repair.Legacy.Dao;

/// <summary>
/// Fake impl that fakes graph query and determining <see cref="CommonBaseAbstractUser"/> type
/// </summary>
public class UserDao
{
  public async Task<CommonBaseAbstractUser> FindBy(long? userId)
  {
    var contract = new SignedContract
    {
      CoveredParts = Enum.GetValues<Part>().ToHashSet(),
      CoverageRatio = 100.0
    };

    var user = new EmployeeDriverWithOwnCar
    {
      Contract = contract
    };
    return await Task.FromResult(user);
  }
}