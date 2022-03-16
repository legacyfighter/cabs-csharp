using System;
using LegacyFighter.Cabs.Contracts.Legacy;

namespace LegacyFighter.CabsTests.Contracts.Legacy;

public class DocumentTest
{
  private const string AnyNumber = "number";
  private static User _anyUser = default!;
  private static User _otherUser = default!;
  private const string Title = "title";

  [SetUp]
  public void SetUp()
  {
    _anyUser = new User();
    _otherUser = new User();
  }

  [Test]
  public void OnlyDraftCanBeVerifiedByUserOtherThanCreator()
  {
    var doc = new Document(AnyNumber, _anyUser);

    doc.VerifyBy(_otherUser);

    Assert.AreEqual(DocumentStatus.Verified, doc.Status);
  }

  [Test]
  public void CanNotChangePublished()
  {
    var doc = new Document(AnyNumber, _anyUser);
    doc.ChangeTitle(Title);
    doc.VerifyBy(_otherUser);
    doc.Publish();

    doc.Invoking(d => d.ChangeTitle(string.Empty)).Should().Throw<InvalidOperationException>();

    Assert.AreEqual(Title, doc.Title);
  }

  [Test]
  public void ChangingVerifiedMovesToDraft()
  {
    var doc = new Document(AnyNumber, _anyUser);
    doc.ChangeTitle(Title);
    doc.VerifyBy(_otherUser);

    doc.ChangeTitle("");

    Assert.AreEqual(DocumentStatus.Draft, doc.Status);
  }
}