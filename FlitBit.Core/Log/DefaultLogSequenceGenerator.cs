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