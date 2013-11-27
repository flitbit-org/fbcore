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