#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Core.Factory
{
  /// <summary>
  ///   Interface for factory providers.
  /// </summary>
  public interface IFactoryProvider
  {
    /// <summary>
    ///   Gets a factory.
    /// </summary>
    /// <returns></returns>
    IFactory GetFactory();
  }
}