#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;

namespace FlitBit.Core.Buffers
{
	internal static class CRC
	{
		internal const int CrcTableLength = 256;
	}

	/// <summary>
	///   Utility class for generating CRC16 checksums.
	/// </summary>
	public class Crc16
	{
		static readonly ushort[] Table = new ushort[CRC.CrcTableLength];

		static Crc16()
		{
			const ushort poly = 0xA001;
			for (ushort i = 0; i < CRC.CrcTableLength; ++i)
			{
				ushort value = 0;
				var temp = i;
				for (byte j = 0; j < 8; ++j)
				{
					if (((value ^ temp) & 0x0001) != 0)
					{
						value = (ushort) ((value >> 1) ^ poly);
					}
					else
					{
						value >>= 1;
					}
					temp >>= 1;
				}
				Table[i] = value;
			}
		}

		/// <summary>
		///   Computes a checksum over an array of bytes.
		/// </summary>
		/// <param name="bytes">the bytes</param>
		/// <returns>the checksum</returns>
		[CLSCompliant(false)]
		public ushort ComputeChecksum(byte[] bytes)
		{
			Contract.Requires<ArgumentNullException>(bytes != null);
			ushort crc = 0;
			var len = bytes.Length;
			for (var i = 0; i < len; ++i)
			{
				var index = (byte) (crc ^ bytes[i]);
				crc = (ushort) ((crc >> 8) ^ Table[index]);
			}
			return crc;
		}
	}

	/// <summary>
	///   Utility class for generating CRC16 checksums.
	/// </summary>
	public class Crc32
	{
		static readonly uint[] Table = new uint[CRC.CrcTableLength];

		static Crc32()
		{
			const uint poly = 0xedb88320;
			for (uint i = 0; i < CRC.CrcTableLength; ++i)
			{
				var temp = i;
				for (var j = 8; j > 0; --j)
				{
					if ((temp & 1) == 1)
					{
						temp = (temp >> 1) ^ poly;
					}
					else
					{
						temp >>= 1;
					}
				}
				Table[i] = temp;
			}
		}

		/// <summary>
		///   Computes a checksum over an array of bytes.
		/// </summary>
		/// <param name="bytes">the bytes</param>
		/// <returns>the checksum</returns>
		[CLSCompliant(false)]
		public uint ComputeChecksum(byte[] bytes)
		{
			Contract.Requires<ArgumentNullException>(bytes != null);
			var crc = 0xffffffff;
			foreach (var b in bytes)
			{
				var index = (byte) (((crc) & 0xff) ^ b);
				crc = (crc >> 8) ^ Table[index];
			}
			return ~crc;
		}

		/// <summary>
		///   Computes a checksum over an array of bytes beginning with the first and
		///   continuing to length.
		/// </summary>
		/// <param name="bytes"></param>
		/// <param name="first"></param>
		/// <param name="length"></param>
		/// <returns></returns>
		[CLSCompliant(false)]
		public uint ComputeChecksum(byte[] bytes, int first, int length)
		{
			var crc = 0xffffffff;
			for (var i = first; i < length; ++i)
			{
				var index = (byte) (((crc) & 0xff) ^ bytes[i]);
				crc = (crc >> 8) ^ Table[index];
			}
			return ~crc;
		}
	}

	/// <summary>
	///   A few common initial CRC values
	/// </summary>
	public enum InitialCrcValue
	{
		/// <summary>
		///   All zero.
		/// </summary>
		Zeros = 0,

		/// <summary>
		///   Common initial value of 0x1D0F
		/// </summary>
		NonZeroX1D0F = 0x1d0f,

		/// <summary>
		///   Common initial value of 0xFFFF
		/// </summary>
		NonZeroXFfff = 0xffff,
	}

	/// <summary>
	///   Utility class for generating CRC16CITT checksums.
	/// </summary>
	public class Crc16Ccitt
	{
		static readonly ushort[] Table = new ushort[CRC.CrcTableLength];

		readonly ushort _initialValue;

		static Crc16Ccitt()
		{
			const ushort poly = 4129;
			for (var i = 0; i < CRC.CrcTableLength; ++i)
			{
				ushort temp = 0;
				var a = (ushort) (i << 8);
				for (var j = 0; j < 8; ++j)
				{
					if (((temp ^ a) & 0x8000) != 0)
					{
						temp = (ushort) ((temp << 1) ^ poly);
					}
					else
					{
						temp <<= 1;
					}
					a <<= 1;
				}
				Table[i] = temp;
			}
		}

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="initialValue">which initial value the checksum should use</param>
		public Crc16Ccitt(InitialCrcValue initialValue) { this._initialValue = (ushort) initialValue; }

		/// <summary>
		///   Computes a checksum over an array of bytes.
		/// </summary>
		/// <param name="bytes">the bytes</param>
		/// <returns>the checksum</returns>
		[CLSCompliant(false)]
		public ushort ComputeChecksum(byte[] bytes)
		{
			Contract.Requires<ArgumentNullException>(bytes != null);
			var crc = this._initialValue;
			var len = bytes.Length;
			for (var i = 0; i < len; ++i)
			{
				crc = (ushort) ((crc << 8) ^ Table[((crc >> 8) ^ (0xff & bytes[i]))]);
			}
			return crc;
		}
	}
}