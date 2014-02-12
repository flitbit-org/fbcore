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