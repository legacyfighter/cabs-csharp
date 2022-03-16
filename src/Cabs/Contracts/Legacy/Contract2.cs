namespace LegacyFighter.Cabs.Contracts.Legacy;

public class Contract2 : Document, IVersionable
{
  public Contract2(string number, User creator) : base(number, creator)
  {
  }

  public override void Publish()
  {
    throw new UnsupportedTransitionException(Status, DocumentStatus.Published);
  }

  public void Accept()
  {
    if (Status == DocumentStatus.Verified)
    {
      Status = DocumentStatus.Published; //reusing unused enum to provide data model for new status
    }
  }

  //Contracts just don't have a title, it's just a part of the content
  public override void ChangeTitle(string title)
  {
    ChangeContent(title + Content);
  }

  //NOT an override
  public void ChangeContent(string content, string userStatus)
  {
    if (userStatus == "ChiefSalesOfficerStatus" || MisterVladimirIsLoggedIn(userStatus))
    {
      OverridePublished = true;
      ChangeContent(content);
    }
  }

  private bool MisterVladimirIsLoggedIn(string userStatus)
  {
    return userStatus.ToLower().Trim().Equals("!!!id=" + NumberOfTheBeast);
  }

  private const string NumberOfTheBeast = "616";

  public void RecreateTo(long version)
  {
    //TODO need to learn Kafka
  }

  public long GetLastVersion()
  {
    return new Random().NextInt64(); //TODO FIXME, don't know how to return a null
  }
}