using System;
using System.Collections.Generic;
using System.Linq;
using LegacyFighter.Cabs.MoneyValue;
using LegacyFighter.Cabs.Repair.Legacy.Job;
using LegacyFighter.Cabs.Repair.Legacy.Parts;
using LegacyFighter.Cabs.Repair.Legacy.User;

namespace LegacyFighter.CabsTests.Repair.Legacy.Job;

public class RepairTest
{
  [Test]
  public void EmployeeDriverWithOwnCarCoveredByWarrantyShouldRepairForFree()
  {
    //given
    var employee = new EmployeeDriverWithOwnCar
    {
      Contract = FullCoverageWarranty()
    };
    //when
    var result = employee.DoJob(FullRepair());
    //then
    Assert.AreEqual(JobResult.Decisions.Accepted, result.Decision);
    Assert.AreEqual(Money.Zero, result.GetParam("totalCost"));
    Assert.AreEqual(AllParts(), result.GetParam("acceptedParts"));
  }

  private RepairJob FullRepair()
  {
    var job = new RepairJob();
    job.EstimatedValue = new Money(50000);
    job.PartsToRepair = AllParts();
    return job;
  }

  private SignedContract FullCoverageWarranty()
  {
    var contract = new SignedContract
    {
      CoverageRatio = 100.0,
      CoveredParts = AllParts()
    };
    return contract;
  }

  private ISet<Part> AllParts()
  {
    return Enum.GetValues<Part>().ToHashSet();
  }
}