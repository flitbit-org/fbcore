﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;

namespace FlitBit.Core.Buffers
{
  /// <summary>
  ///   Reflects type T onto a buffer.
  /// </summary>
  /// <typeparam name="T">type T</typeparam>
  [CLSCompliant(false)]
  public interface IBufferReflector<T>
  {
    /// <summary>
    ///   Reads an instance of type T to the buffer.
    /// </summary>
    /// <param name="reader">a buffer reader</param>
    /// <param name="source">the source buffer</param>
    /// <param name="offset">
    ///   reference to an offset into the buffer where reading
    ///   can begin; upon exit, must be incremented by the number of bytes consumed
    /// </param>
    /// <param name="target">variable that will hold the instance upon success</param>
    /// <returns>the number of bytes consumed during the read</returns>
    int ReadFromBuffer(IBufferReader reader, byte[] source, ref int offset, out T target);

    /// <summary>
    ///   Writes an instance of type T to the buffer.
    /// </summary>
    /// <param name="writer">a buffer writer</param>
    /// <param name="target">the target buffer</param>
    /// <param name="offset">
    ///   reference to an offset into the buffer where writing
    ///   can begin; upon exit, must be incremented by the number of bytes consumed
    /// </param>
    /// <param name="source">the instance of type T being written</param>
    /// <returns>total number of bytes written</returns>
    int WriteToBuffer(IBufferWriter writer, byte[] target, ref int offset, T source);
  }
}