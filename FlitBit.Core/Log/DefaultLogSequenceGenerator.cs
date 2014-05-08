#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Threading;

namespace FlitBit.Core.Log
{
  /// <summary>
  /// Default log sequence generator.
  /// </summary>
  public sealed class DefaultLogSequenceGenerator : ILogSequenceGenerator
  {
    static int __processSequence;

    /// <summary>
    /// Generates the next sequence number.
    /// </summary>
    public int Next { get { return Interlocked.Increment(ref __processSequence); } }
  }
}