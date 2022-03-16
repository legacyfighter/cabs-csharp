using Core.Maybe;

namespace LegacyFighter.Cabs.Parties.Utils;

public class PolymorphicDictionary<TValue> where TValue : notnull
{
  private readonly IDictionary<Type, TValue?> _inner = new Dictionary<Type, TValue?>();
  
  public bool ContainsKey(Type key)
  {
    return FindEntry(key).HasValue;
  }

  public TValue? this[Type key]
  {
    get => FindEntry(key).Select(e => e.Value).OrElseDefault();
    set => _inner[key] = value;
  }

  private Maybe<KeyValuePair<Type, TValue>> FindEntry(Type key)
  {
    return _inner.Where(kvp => key.IsAssignableFrom(kvp.Key)).FirstMaybe();
  }
}