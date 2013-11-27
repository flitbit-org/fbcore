namespace FlitBit.Core.Factory
{
  internal sealed class DefaultFactoryProvider : IFactoryProvider
  {
    readonly IFactory _factory = new DefaultFactory();

    #region IFactoryProvider Members

    public IFactory GetFactory() { return _factory; }

    #endregion
  }
}