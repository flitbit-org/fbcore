﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System.Runtime.InteropServices;

namespace FlitBit.Core.Buffers
{
  /// <summary>
  ///   Utility struct to convert between Int32 and Single
  /// </summary>
  [StructLayout(LayoutKind.Explicit)]
  internal struct Int32SingleUnion
  {
    [FieldOffset(0)]
    readonly float f;

    [FieldOffset(0)]
    readonly int i;

    /// <summary>
    ///   Creates an instance initialized with the given integer.
    /// </summary>
    /// <param name="i">An Int32 value.</param>
    internal Int32SingleUnion(int i)
    {
      this.f = 0f;
      this.i = i;
    }

    /// <summary>
    ///   Creates an instance initialized with the given floating point number.
    /// </summary>
    /// <param name="f">A Single value.</param>
    internal Int32SingleUnion(float f)
    {
      this.i = 0;
      this.f = f;
    }

    /// <summary>
    ///   Returns the value of the instance as an Int32.
    /// </summary>
    internal int AsInt32 { get { return i; } }

    /// <summary>
    ///   Returns the value of the instance as a Single.
    /// </summary>
    internal float AsSingle { get { return f; } }
  }
}