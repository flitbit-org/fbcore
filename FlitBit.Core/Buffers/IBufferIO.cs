﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.Core.Buffers
{
  /// <summary>
  ///   Interface for objects that provide their own buffer IO.
  /// </summary>
  [CLSCompliant(false)]
  public interface IBufferIO
  {
    /// <summary>
    ///   Reads from the buffer.
    /// </summary>
    /// <param name="reader">a buffer reader</param>
    /// <param name="source">the source buffer</param>
    /// <param name="offset">
    ///   reference to an offset into the buffer where reading
    ///   can begin; upon exit, must be incremented by the number of bytes consumed
    /// </param>
    /// <returns>the number of bytes consumed during the read</returns>
    int ReadFromBuffer(IBufferReader reader, byte[] source, ref int offset);

    /// <summary>
    ///   Writes to the buffer.
    /// </summary>
    /// <param name="writer">a buffer writer</param>
    /// <param name="target">the target buffer</param>
    /// <param name="offset">
    ///   reference to an offset into the buffer where writing
    ///   can begin; upon exit, must be incremented by the number of bytes consumed
    /// </param>
    /// <returns>total number of bytes written</returns>
    int WriteToBuffer(IBufferWriter writer, byte[] target, ref int offset);
  }
}