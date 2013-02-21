#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.
// For licensing information see License.txt (MIT style licensing).
#endregion

using System;
using System.Text;
using System.Diagnostics.Contracts;
using FlitBit.Core.Properties;

namespace FlitBit.Core.Buffers
{
	/// <summary>
	/// Helper for reading binary data from a buffer.
	/// </summary>
	[CLSCompliant(false)]
	[ContractClass(typeof(CodeContracts.ContractForIBufferReader))]		
	public interface IBufferReader
	{
		/// <summary>
		/// Gets the encoding used when reading string data.
		/// </summary>
		Encoding Encoding { get; }
		/// <summary>
		/// Reads a boolean from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		bool ReadBoolean(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads a byte from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		byte ReadByte(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads an array of bytes from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <param name="count">the number of bytes in the array</param>
		/// <returns>a value</returns>
		byte[] ReadBytes(byte[] buffer, ref int offset, int count);
		/// <summary>
		/// Reads a char from the buffer (two-byte).
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		char ReadChar(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads an array of characters from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <param name="arrayLength">the number of characters in the array</param>
		/// <returns>a value</returns>
		char[] ReadCharArray(byte[] buffer, ref int offset, int arrayLength);
		/// <summary>
		/// Reads a decimal from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		decimal ReadDecimal(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads a double from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		double ReadDouble(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads an encoded string from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <param name="byteCount">the number of bytes used by the string</param>
		/// <param name="coder">an encoding used to interpret the bytes</param>
		/// <returns>a value</returns>
		string ReadEncodedString(byte[] buffer, ref int offset, int byteCount, Encoding coder);
		/// <summary>
		/// Reads a Guid from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		Guid ReadGuid(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads an Int16 from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		short ReadInt16(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads an Int32 from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		int ReadInt32(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads an UInt64 from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		long ReadInt64(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads a signed byte from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		sbyte ReadSByte(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads a Single from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		float ReadSingle(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads a length-prefixed string from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		string ReadStringWithByteCountPrefix(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads a length-prefixed string from the buffer using the encoding given.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <param name="coder">an encoding used to interpret the bytes read</param>
		/// <returns>a value</returns>
		string ReadStringWithByteCountPrefix(byte[] buffer, ref int offset, Encoding coder);
		/// <summary>
		/// Reads an UInt16 from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		ushort ReadUInt16(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads an UInt32 from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		uint ReadUInt32(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads an UInt64 from the buffer.
		/// </summary>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <returns>a value</returns>
		ulong ReadUInt64(byte[] buffer, ref int offset);
		/// <summary>
		/// Reads an instance of type T from the buffer.
		/// </summary>
		/// <typeparam name="T">type T</typeparam>
		/// <param name="buffer">the buffer</param>
		/// <param name="offset">offest into buffer where reading begins</param>
		/// <param name="reflector">reflector for reading type T</param>
		/// <returns>the instance of type T read from the buffer</returns>
		T ReadReflectedObject<T>(byte[] buffer, ref int offset, IBufferReflector<T> reflector);	
	}

	namespace CodeContracts
	{
		/// <summary>
		/// CodeContracts Class for IBufferReader
		/// </summary>
		[ContractClassFor(typeof(IBufferReader))]
		internal abstract class ContractForIBufferReader : IBufferReader
		{
			public Encoding Encoding
			{
				get
				{
					Contract.Ensures(Contract.Result<Encoding>() != null);

					throw new NotImplementedException();
				}
			}

			public bool ReadBoolean(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(sizeof(bool) <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
			
				throw new NotImplementedException();
			}

			public byte ReadByte(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(sizeof(byte) <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public byte[] ReadBytes(byte[] buffer, ref int offset, int count)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(count <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public char ReadChar(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(sizeof(char) <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public char[] ReadCharArray(byte[] buffer, ref int offset, int arrayLength)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>((sizeof(char) * arrayLength) <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public decimal ReadDecimal(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(sizeof(decimal) <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public double ReadDouble(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(sizeof(double) <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public string ReadEncodedString(byte[] buffer, ref int offset, int byteCount, Encoding coder)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(byteCount <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				Contract.Requires<ArgumentNullException>(coder != null);
				
				throw new NotImplementedException();
			}

			public Guid ReadGuid(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(16 <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public short ReadInt16(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(sizeof(short) <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public int ReadInt32(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(sizeof(int) <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public long ReadInt64(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(sizeof(long) <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public sbyte ReadSByte(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(sizeof(sbyte) <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public float ReadSingle(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(sizeof(float) <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public string ReadStringWithByteCountPrefix(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				
				throw new NotImplementedException();
			}

			public string ReadStringWithByteCountPrefix(byte[] buffer, ref int offset, Encoding coder)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentNullException>(coder != null);
				
				throw new NotImplementedException();
			}

			public ushort ReadUInt16(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(sizeof(ushort) <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public uint ReadUInt32(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(sizeof(uint) <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public ulong ReadUInt64(byte[] buffer, ref int offset)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentOutOfRangeException>(sizeof(ulong) <= buffer.Length - offset, Resources.Chk_OffsetWouldResultInBufferOverrun);
				
				throw new NotImplementedException();
			}

			public T ReadReflectedObject<T>(byte[] buffer, ref int offset, IBufferReflector<T> reflector)
			{
				Contract.Requires<ArgumentNullException>(buffer != null);
				Contract.Requires<ArgumentOutOfRangeException>(offset >= 0);
				Contract.Requires<ArgumentNullException>(reflector != null);
				
				throw new NotImplementedException();
			}
		}
	}
}
