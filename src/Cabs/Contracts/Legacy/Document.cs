namespace LegacyFighter.Cabs.Contracts.Legacy;

public class Document : BaseAggregateRoot, IPrintable
{
  private string Number { get; set; }
  protected virtual ISet<User> AssignedUsers { get; set; }
  protected virtual User Creator { get; set; }
  protected virtual User Verifier { get; set; }
  protected string Content { get; private set; }
  public DocumentStatus Status { get; protected set; } = DocumentStatus.Draft;
  public string Title { get; private set; }

  public Document(string number, User creator)
  {
    Number = number;
    Creator = creator;
  }

  protected Document()
  {
  }

  public void VerifyBy(User verifier)
  {
    if (Status != DocumentStatus.Draft)
    {
      throw new InvalidOperationException("Can not verify in status: " + Status);
    }

    if (Creator.Equals(verifier))
    {
      throw new ArgumentException("Verifier can not verify documents by himself");
    }
    Verifier = verifier;
    Status = DocumentStatus.Verified;
  }

  public virtual void Publish()
  {
    if (Status != DocumentStatus.Verified)
    {
      throw new InvalidOperationException("Can not publish in status: " + Status);
    }
    Status = DocumentStatus.Published;
  }

  public void Archive()
  {
    Status = DocumentStatus.Archived;
  }

  //===============================================================

  public virtual void ChangeTitle(string title)
  {
    if (Status == DocumentStatus.Archived || Status == DocumentStatus.Published)
    {
      throw new InvalidOperationException("Can not change title in status: " + Status);
    }
    Title = title;
    if (Status == DocumentStatus.Verified)
    {
      Status = DocumentStatus.Draft;
    }
  }

  protected bool OverridePublished;

  public void ChangeContent(string content)
  {
    if (OverridePublished)
    {
      Content = content;
      return;
    }

    if (Status == DocumentStatus.Archived || Status == DocumentStatus.Published)
    {
      throw new InvalidOperationException("Can not change content in status: " + Status);
    }
    Content = content;
    if (Status == DocumentStatus.Verified)
    {
      Status = DocumentStatus.Draft;
    }
  }

  //===============================================================
}