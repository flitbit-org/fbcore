﻿#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Text;
using FlitBit.Core.Properties;

namespace FlitBit.Core.Buffers
{
  /// <summary>
  ///   IBufferWriter's base implementation
  /// </summary>
  public abstract class BufferWriter : IBufferWriter
  {
    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    protected BufferWriter()
      : this(Encoding.Unicode)
    {}

    /// <summary>
    ///   Creates a new instance.
    /// </summary>
    /// <param name="enc">the encoding used to produce bytes for strings.</param>
    protected BufferWriter(Encoding enc)
    {
      Contract.Requires<ArgumentNullException>(enc != null);
      this.Encoding = enc;
    }

    #region IBufferWriter Members

    /// <summary>
    ///   Gets the encoding used when writing string data.
    /// </summary>
    public Encoding Encoding { get; private set; }

    /// <summary>
    ///   Fills a buffer.
    /// </summary>
    /// <param name="buffer">a buffer</param>
    /// <param name="offset">offset to begin</param>
    /// <param name="count">number of bytes to fill</param>
    /// <param name="value">fill value</param>
    public void FillBytes(byte[] buffer, ref int offset, int count, byte value)
    {
      var last = offset + count - 1;
      while (offset <= last)
      {
        buffer[offset++] = value;
      }
    }

    /// <summary>
    ///   Writes a boolean value to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, bool value)
    {
      buffer[offset++] = (byte)((value) ? 1 : 0);
      return sizeof(byte);
    }

    /// <summary>
    ///   Writes a byte value to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, byte value)
    {
      buffer[offset++] = value;
      return sizeof(byte);
    }

    /// <summary>
    ///   Writes a byte array to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the array</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, byte[] value)
    {
      Array.Copy(value, 0, buffer, offset, value.Length);
      offset += value.Length;
      return value.Length;
    }

    /// <summary>
    ///   Writes from a byte array to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the source array</param>
    /// <param name="sourceOffset">offset into source where copying begins</param>
    /// <param name="count">number of bytes to copy from source into buffer</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, byte[] value, int sourceOffset, int count)
    {
      if (value != null)
      {
        Array.Copy(value, sourceOffset, buffer, offset, count);
        offset += count;
      }
      return count;
    }

    /// <summary>
    ///   Writes a char value to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, char value)
    {
      return Write(buffer, ref offset, (Int16)value);
    }

    /// <summary>
    ///   Writes an Int16 to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, short value)
    {
      return Write(buffer, ref offset, (ushort)value);
    }

    /// <summary>
    ///   Writes an Int32 to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, int value)
    {
      return Write(buffer, ref offset, (uint)value);
    }

    /// <summary>
    ///   Writes an Int64 to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, long value)
    {
      return Write(buffer, ref offset, (ulong)value);
    }

    /// <summary>
    ///   Writes a decimal value to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, decimal value)
    {
      var written = 0;
// ReSharper disable LoopCanBeConvertedToQuery
      foreach (var b in Decimal.GetBits(value))
      {
        written += Write(buffer, ref offset, b);
      }
// ReSharper restore LoopCanBeConvertedToQuery
      return written;
    }

    /// <summary>
    ///   Writes a floating point double value to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, double value)
    {
      return Write(buffer, ref offset, BitConverter.DoubleToInt64Bits(value));
    }

    /// <summary>
    ///   Writes a floating point single value to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, float value)
    {
      return Write(buffer, ref offset, new Int32SingleUnion(value).AsInt32);
    }

    /// <summary>
    ///   Writes a signed byte to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <returns>number of bytes written</returns>
    [CLSCompliant(false)]
    public int Write(byte[] buffer, ref int offset, sbyte value)
    {
      return Write(buffer, ref offset, (byte)value);
    }

    /// <summary>
    ///   Writes a UInt16 value to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <returns>number of bytes written</returns>
    [CLSCompliant(false)]
    public virtual int Write(byte[] buffer, ref int offset, ushort value)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    ///   Writes a UInt32 value to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <returns>number of bytes written</returns>
    [CLSCompliant(false)]
    public virtual int Write(byte[] buffer, ref int offset, uint value)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    ///   Writes a UInt64 value to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <returns>number of bytes written</returns>
    [CLSCompliant(false)]
    public virtual int Write(byte[] buffer, ref int offset, ulong value)
    {
      throw new NotImplementedException();
    }

    /// <summary>
    ///   Writes a Guid value to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, Guid value)
    {
      return Write(buffer, ref offset, value.ToByteArray());
    }

    /// <summary>
    ///   Writes a string to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the string</param>
    /// <param name="byteLengthPrefix">whether a byte length prefix should be written</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, string value, bool byteLengthPrefix)
    {
      return Write(buffer, ref offset, value, byteLengthPrefix, this.Encoding);
    }

    /// <summary>
    ///   Writes a string to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the string</param>
    /// <param name="byteLengthPrefix">whether a byte length prefix should be written</param>
    /// <param name="coder">an encoding used to transform the string to bytes</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, string value, bool byteLengthPrefix, Encoding coder)
    {
      var startingPos = offset;
      var encodedLength = coder.GetByteCount(value);
      if (byteLengthPrefix)
      {
        if (((buffer.Length - offset) + sizeof(Int32)) < encodedLength)
        {
          throw new InvalidOperationException(Resources.Chk_OffsetWouldResultInBufferOverrun);
        }
        Write(buffer, ref offset, encodedLength);
      }
      else
      {
        if ((buffer.Length - offset) < encodedLength)
        {
          throw new InvalidOperationException(Resources.Chk_OffsetWouldResultInBufferOverrun);
        }
      }

      offset += coder.GetBytes(value, 0, value.Length, buffer, offset);
      return offset - startingPos;
    }

    /// <summary>
    ///   Writes an array of chars to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <param name="byteLengthPrefix">whether a byte length prefix should be written</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, Char[] value, bool byteLengthPrefix)
    {
      return Write(buffer, ref offset, value, byteLengthPrefix, this.Encoding);
    }

    /// <summary>
    ///   Writes an array of chars to the buffer.
    /// </summary>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="value">the value</param>
    /// <param name="byteLengthPrefix">whether a byte length prefix should be written</param>
    /// <param name="coder">an encoding used to transform the string to bytes</param>
    /// <returns>number of bytes written</returns>
    public int Write(byte[] buffer, ref int offset, Char[] value, bool byteLengthPrefix, Encoding coder)
    {
      var startingPos = offset;
      var encodedLength = coder.GetByteCount(value);
      if (byteLengthPrefix)
      {
        if (((buffer.Length - offset) + sizeof(Int32)) < encodedLength)
        {
          throw new InvalidOperationException(Resources.Chk_OffsetWouldResultInBufferOverrun);
        }
        Write(buffer, ref offset, encodedLength);
      }
      else
      {
        if ((buffer.Length - offset) < encodedLength)
        {
          throw new InvalidOperationException(Resources.Chk_OffsetWouldResultInBufferOverrun);
        }
      }

      offset += coder.GetBytes(value, 0, value.Length, buffer, offset);
      return offset - startingPos;
    }

    /// <summary>
    ///   Writes an instance of type T to the buffer.
    /// </summary>
    /// <typeparam name="T">type T</typeparam>
    /// <param name="buffer">the buffer</param>
    /// <param name="offset">offest into buffer where writing begins</param>
    /// <param name="reflector">reflector for writing type T</param>
    /// <param name="value">the instance</param>
    /// <returns>number of bytes written</returns>
    [CLSCompliant(false)]
    public int WriteReflectedObject<T>(byte[] buffer, ref int offset, IBufferReflector<T> reflector, T value)
    {
      return reflector.WriteToBuffer(this, buffer, ref offset, value);
    }

    #endregion

    /// <summary>
    ///   Creates the default buffer writer.
    /// </summary>
    /// <returns>
    ///   a buffer writer matching the current machine's
    ///   bit-endianness.
    /// </returns>
    [CLSCompliant(false)]
    public static IBufferWriter Create()
    {
      return (BitConverter.IsLittleEndian)
               ? new LittleEndianBufferWriter()
               : (IBufferWriter)new BigEndianBufferWriter();
    }

    /// <summary>
    ///   Creates the default buffer writer.
    /// </summary>
    /// <param name="enc">the encoding used to produce bytes for strings.</param>
    /// <returns>
    ///   a buffer writer matching the current machine's
    ///   bit-endianness.
    /// </returns>
    [CLSCompliant(false)]
    public static IBufferWriter Create(Encoding enc)
    {
      return (BitConverter.IsLittleEndian)
               ? new LittleEndianBufferWriter(enc)
               : (IBufferWriter)new BigEndianBufferWriter(enc);
    }
  }
}