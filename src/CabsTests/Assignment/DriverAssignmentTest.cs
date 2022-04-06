using System;
using LegacyFighter.Cabs.Assignment;
using NodaTime;

namespace LegacyFighter.CabsTests.Assignment;

public class DriverAssignmentTest
{
  private const long DriverId = 1L;
  private const long SecondDriverId = 2L;

  [Test]
  public void CanAcceptTransit()
  {
    //given
    var assignment = AssignmentForTransit(Now());
    //and
    assignment.ProposeTo(DriverId);

    //when
    assignment.AcceptBy(DriverId);
    //then
    Assert.AreEqual(AssignmentStatuses.OnTheWay, assignment.Status);
  }


  [Test]
  public void OnlyOneDriverCanAcceptTransit()
  {
    //given
    var assignment = AssignmentForTransit(Now());
    //and
    assignment.ProposeTo(DriverId);
    //and
    assignment.AcceptBy(DriverId);

    //expect
    assignment.Invoking(a => a.AcceptBy(SecondDriverId))
      .Should().Throw<InvalidOperationException>();
  }

  [Test]
  public void TransitCannotByAcceptedByDriverWhoAlreadyRejected()
  {
    //given
    var assignment = AssignmentForTransit(Now());
    //and
    assignment.RejectBy(DriverId);

    //expect
    assignment.Invoking(a => a.AcceptBy(DriverId))
      .Should().Throw<InvalidOperationException>();
  }

  [Test]
  public void TransitCannotByAcceptedByDriverWhoHasNotSeenProposal()
  {
    //given
    var assignment = AssignmentForTransit(Now());

    //expect
    assignment.Invoking(a => a.AcceptBy(DriverId))
      .Should().Throw<InvalidOperationException>();
  }


  [Test]
  public void CanRejectTransit()
  {
    //given
    var assignment = AssignmentForTransit(Now());

    //when
    assignment.RejectBy(DriverId);

    //then
    Assert.AreEqual(AssignmentStatuses.WaitingForDriverAssignment, assignment.Status);
  }

  private DriverAssignment AssignmentForTransit(Instant when)
  {
    return new DriverAssignment(Guid.NewGuid(), when);
  }

  private static Instant Now() => SystemClock.Instance.GetCurrentInstant();
}