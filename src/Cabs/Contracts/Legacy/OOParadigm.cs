namespace LegacyFighter.Cabs.Contracts.Legacy;

internal abstract class OoParadigm
{
  //2. enkapsulacja - ukrycie impl
  private object _filed;

  //1. abstrakcja - agent odbierający sygnały
  public void Method()
  {
    //do sth
  }

  //3. polimorfizm - zmienne zachowania
  protected abstract void AbstractStep();
}

//4. dziedziczenie - technika wspierająca polimorfizm
internal class ConcreteType : OoParadigm
{
  protected override void AbstractStep()
  {

  }
}