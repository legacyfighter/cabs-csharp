namespace LegacyFighter.Cabs.Contracts.Model;

public class ContentId : IEquatable<ContentId>
{
  private readonly Guid _contentId;

  protected ContentId()
  {
  }

  public ContentId(Guid contentId)
  {
    _contentId = contentId;
  }

  public override string ToString()
  {
    return "ContentId{" +
           "contentId=" + _contentId +
           '}';
  }

  public bool Equals(ContentId other)
  {
    if (ReferenceEquals(null, other)) return false;
    if (ReferenceEquals(this, other)) return true;
    return _contentId.Equals(other._contentId);
  }

  public override bool Equals(object obj)
  {
    if (ReferenceEquals(null, obj)) return false;
    if (ReferenceEquals(this, obj)) return true;
    if (obj.GetType() != GetType()) return false;
    return Equals((ContentId)obj);
  }

  public override int GetHashCode()
  {
    return _contentId.GetHashCode();
  }

  public static bool operator ==(ContentId left, ContentId right)
  {
    return Equals(left, right);
  }

  public static bool operator !=(ContentId left, ContentId right)
  {
    return !Equals(left, right);
  }
}