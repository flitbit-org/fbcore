using System;

namespace FlitBit.Core
{
  /// <summary>
  ///   Interface for disposable objects that can be interrogated about their disposed state.
  /// </summary>
  public interface IInterrogateDisposable : IDisposable
  {
    /// <summary>
    ///   Indicates whether the disposable has been disposed.
    /// </summary>
    bool IsDisposed { get; }
  }
}