#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Text;

namespace FlitBit.Core.Buffers
{
	/// <summary>
	///   IBufferReader's base implementation
	/// </summary>
	[CLSCompliant(false)]
	public abstract class BufferReader : IBufferReader
	{
		/// <summary>
		///   Creates a new instance.
		/// </summary>
		protected BufferReader()
			: this(Encoding.Unicode)
		{}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="enc">an encoding</param>
		protected BufferReader(Encoding enc)
		{
			Contract.Requires<ArgumentNullException>(enc != null);

			Encoding = enc;
		}

		#region IBufferReader Members

		/// <summary>
		///   Gets the reader's encoding. This is the encoding used to read string data from the buffer.
		/// </summary>
		public Encoding Encoding { get; private set; }

		/// <summary>
		///   Reads a boolean from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		public bool ReadBoolean(byte[] buffer, ref int offset)
		{
			return buffer[offset++] == 1;
		}

		/// <summary>
		///   Reads a byte array from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a byte array</returns>
		public byte ReadByte(byte[] buffer, ref int offset)
		{
			return buffer[offset++];
		}

		/// <summary>
		///   Reads a byte array from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <param name="count">number of bytes to read</param>
		/// <returns>a byte array</returns>
		public byte[] ReadBytes(byte[] buffer, ref int offset, int count)
		{
			var result = new byte[count];
			if (count > 0)
			{
				Array.Copy(buffer, offset, result, 0, count);
				offset += count;
			}
			return result;
		}

		/// <summary>
		///   Reads a char from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a char value</returns>
		public char ReadChar(byte[] buffer, ref int offset)
		{
			unchecked
			{
				return (char) ReadUInt16(buffer, ref offset);
			}
		}

		/// <summary>
		///   Reads an Int16 from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>an int16 value</returns>
		public short ReadInt16(byte[] buffer, ref int offset)
		{
			unchecked
			{
				return (short) ReadUInt16(buffer, ref offset);
			}
		}

		/// <summary>
		///   Reads an Int32 from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>an Int32 value</returns>
		public int ReadInt32(byte[] buffer, ref int offset)
		{
			unchecked
			{
				return (int) ReadUInt32(buffer, ref offset);
			}
		}

		/// <summary>
		///   Reads an Int64 from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>an Int64 value</returns>
		public long ReadInt64(byte[] buffer, ref int offset)
		{
			unchecked
			{
				return (long) ReadUInt64(buffer, ref offset);
			}
		}

		/// <summary>
		///   Reads a decimal from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a decimal value</returns>
		public decimal ReadDecimal(byte[] buffer, ref int offset)
		{
			var bits = new int[4];
			bits[0] = ReadInt32(buffer, ref offset);
			bits[1] = ReadInt32(buffer, ref offset);
			bits[2] = ReadInt32(buffer, ref offset);
			bits[3] = ReadInt32(buffer, ref offset);
			return new decimal(bits);
		}

		/// <summary>
		///   Reads a double from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a double value</returns>
		public double ReadDouble(byte[] buffer, ref int offset)
		{
			return BitConverter.Int64BitsToDouble(ReadInt64(buffer, ref offset));
		}

		/// <summary>
		///   Reads a single from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a single value</returns>
		public float ReadSingle(byte[] buffer, ref int offset)
		{
			return new Int32SingleUnion(ReadInt32(buffer, ref offset)).AsSingle;
		}

		/// <summary>
		///   Reads a signed byte from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a signed byte</returns>
		public sbyte ReadSByte(byte[] buffer, ref int offset)
		{
			return (sbyte) ReadByte(buffer, ref offset);
		}

		/// <summary>
		///   Reads a UInt32 from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a UInt32 value</returns>
		public virtual ushort ReadUInt16(byte[] buffer, ref int offset)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Reads a UInt32 from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a UInt32 value</returns>
		public virtual uint ReadUInt32(byte[] buffer, ref int offset)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Reads a UInt64 from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a UInt64 value</returns>
		public virtual ulong ReadUInt64(byte[] buffer, ref int offset)
		{
			throw new NotImplementedException();
		}

		/// <summary>
		///   Reads a Guid from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a guid value</returns>
		public Guid ReadGuid(byte[] buffer, ref int offset)
		{
			return new Guid(ReadBytes(buffer, ref offset, 16));
		}

		/// <summary>
		///   Reads an length-prefixed string from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a string value</returns>
		public string ReadStringWithByteCountPrefix(byte[] buffer, ref int offset)
		{
			return ReadStringWithByteCountPrefix(buffer, ref offset, this.Encoding);
		}

		/// <summary>
		///   Reads an length-prefixed, encoded string from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <param name="coder">an encoding to use when decoding the bytes</param>
		/// <returns>a string value</returns>
		public string ReadStringWithByteCountPrefix(byte[] buffer, ref int offset, Encoding coder)
		{
			var byteCount = ReadInt32(buffer, ref offset);
			return ReadEncodedString(buffer, ref offset, byteCount, coder);
		}

		/// <summary>
		///   Reads an encoded string from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <param name="byteCount">number of bytes to interpret as string data</param>
		/// <param name="coder">an encoding to use when decoding the bytes</param>
		/// <returns>a string value</returns>
		public string ReadEncodedString(byte[] buffer, ref int offset, int byteCount, Encoding coder)
		{
			var chars = coder.GetChars(buffer, offset, byteCount);
			offset += byteCount;
			return new String(chars);
		}

		/// <summary>
		///   Reads an array of characters from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <param name="arrayLength">number of characters to read</param>
		/// <returns>a char array</returns>
		public char[] ReadCharArray(byte[] buffer, ref int offset, int arrayLength)
		{
			var result = new Char[arrayLength];
			for (var i = 0; i < arrayLength; i++)
			{
				result[i] = ReadChar(buffer, ref offset);
			}
			return result;
		}

		/// <summary>
		///   Reads an instance of type T from the buffer.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <param name="reflector">reflector for reading type T</param>
		/// <returns>the instance of type T read from the buffer</returns>
		public T ReadReflectedObject<T>(byte[] buffer, ref int offset, IBufferReflector<T> reflector)
		{
			T value;
			var bytesConsumed = reflector.ReadFromBuffer(this, buffer, ref offset, out value);
			return (bytesConsumed > 0) ? value : default(T);
		}

		#endregion

		/// <summary>
		///   Creates the default buffer reader.
		/// </summary>
		/// <returns>
		///   a buffer reader matching the current machine's
		///   bit-endianness.
		/// </returns>
		public static IBufferReader Create()
		{
			return (BitConverter.IsLittleEndian)
				? new LittleEndianBufferReader()
				: (IBufferReader) new BigEndianBufferReader();
		}

		/// <summary>
		///   Creates the default buffer reader.
		/// </summary>
		/// <param name="enc">the encoding used to interpret strings.</param>
		/// <returns>
		///   a buffer reader matching the current machine's
		///   bit-endianness.
		/// </returns>
		public static IBufferReader Create(Encoding enc)
		{
			return (BitConverter.IsLittleEndian)
				? new LittleEndianBufferReader(enc)
				: (IBufferReader) new BigEndianBufferReader(enc);
		}
	}
}