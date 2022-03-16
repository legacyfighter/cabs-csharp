using System.Linq;
using LegacyFighter.Cabs.Contracts.Application.Acme.Dynamic;
using LegacyFighter.Cabs.Contracts.Model;

namespace LegacyFighter.CabsTests.Contracts.Application.Dynamic;

public class DocumentOperationResultAssert
{
  private readonly DocumentOperationResult _result;

  public DocumentOperationResultAssert(DocumentOperationResult result)
  {
    _result = result;
    Assert.AreEqual(DocumentOperationResult.Results.Success, result.Result);
  }

  public DocumentOperationResultAssert Editable()
  {
    Assert.True(_result.IsContentChangePossible);
    return this;
  }

  public DocumentOperationResultAssert Uneditable()
  {
    Assert.False(_result.IsContentChangePossible);
    return this;
  }

  public DocumentOperationResultAssert State(string state)
  {
    Assert.AreEqual(state, _result.StateName);
    return this;
  }

  public DocumentOperationResultAssert Content(ContentId contentId)
  {
    Assert.AreEqual(contentId, _result.ContentId);
    return this;
  }

  public DocumentOperationResultAssert PossibleNextStates(params string[] states)
  {
    CollectionAssert.AreEqual(states.ToHashSet(),
      _result.PossibleTransitionsAndRules.Keys);
    return this;
  }
}