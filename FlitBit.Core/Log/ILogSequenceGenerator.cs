#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

namespace FlitBit.Core.Log
{
  /// <summary>
  /// Generates sequential identities for trace events originating from the process.
  /// </summary>
  public interface ILogSequenceGenerator
  {
    /// <summary>
    /// Generates the next identifier in the sequence.
    /// </summary>
    int Next { get; }
  }
}