#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

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