#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.Core.Parallel
{
  /// <summary>
  ///   Event args for uncaught exceptions.
  /// </summary>
  public class UncaughtExceptionArgs : EventArgs
  {
    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="e"></param>
    public UncaughtExceptionArgs(Exception e) { this.Error = e; }

    /// <summary>
    ///   The uncaught exception.
    /// </summary>
    public Exception Error { get; private set; }
  }
}