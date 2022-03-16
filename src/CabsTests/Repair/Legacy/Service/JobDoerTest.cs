using System;
using System.Collections.Generic;
using System.Linq;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Repair.Legacy.Job;
using LegacyFighter.Cabs.Repair.Legacy.Parts;
using LegacyFighter.Cabs.Repair.Legacy.Service;
using LegacyFighter.CabsTests.Common;

namespace LegacyFighter.CabsTests.Repair.Legacy.Service;

public class JobDoerTest
{
  private CabsApp _app = default!;
  /// <summary>
  /// fake database returns <see cref="Cabs.Repair.Legacy.User.EmployeeDriverWithOwnCar"/>
  /// </summary>
  private static readonly long? AnyUser = 1L;
  private IJobDoer JobDoer => _app.JobDoer;

  [SetUp]
  public void InitializeApp()
  {
    _app = CabsApp.CreateInstance();
  }

  [TearDown]
  public async Task DisposeOfApp()
  {
    await _app.DisposeAsync();
  }

  [Test]
  public async Task EmployeeWithOwnCarWithWarrantyShouldHaveCoveredAllPartsForFree()
  {
    var result = await JobDoer.Repair(AnyUser, RepairJob());

    Assert.AreEqual(result.Decision, JobResult.Decisions.Accepted);
    Assert.AreEqual(result.GetParam("acceptedParts"), AllParts());
    Assert.AreEqual(result.GetParam("totalCost"), Money.Zero);
  }

  private RepairJob RepairJob()
  {
    return new RepairJob
    {
      PartsToRepair = AllParts(),
      EstimatedValue = new Money(7000)
    };
  }

  private ISet<Part> AllParts()
  {
    return Enum.GetValues<Part>().ToHashSet();
  }
}