using FlitBit.Core.Factory;

namespace FlitBit.Core
{
  /// <summary>
  ///   Accesses the current factory.
  /// </summary>
  public static class FactoryProvider
  {
    static IFactoryProvider __provider;

    /// <summary>
    ///   Gets the current factory provider.
    /// </summary>
    public static IFactoryProvider Current
    {
      get { return Util.NonBlockingLazyInitializeVolatile(ref __provider, () => new DefaultFactoryProvider()); }
    }

    /// <summary>
    ///   Gets and sets the global factory.
    /// </summary>
    public static IFactory Factory { get { return Current.GetFactory(); } }

    /// <summary>
    ///   Sets the factory provider.
    /// </summary>
    /// <param name="provider"></param>
    public static void SetFactoryProvider(IFactoryProvider provider)
    {
      var prev = Util.VolatileRead(ref __provider);
      if (prev == provider)
      {
        return;
      }
      Util.VolatileWrite(out __provider, provider);
      if (prev != null
          && provider != null)
      {
        provider.GetFactory()
                .Next = prev.GetFactory();
      }
    }
  }
}