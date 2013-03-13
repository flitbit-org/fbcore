#region COPYRIGHT© 2009-2013 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Diagnostics.Contracts;
using System.Runtime.InteropServices;
using System.Text;
using FlitBit.Core.Properties;

namespace FlitBit.Core.Collections
{
	/// <summary>
	///   Utility structure for working with bit values.
	/// </summary>
	[StructLayout(LayoutKind.Sequential), Serializable]
	public struct BitVector : IEquatable<BitVector>, ICloneable
	{
		/// <summary>
		///   Empty vector; all bits turned off.
		/// </summary>
		public static readonly BitVector Empty = new BitVector(0);

		static readonly int CHashCodeSeed = typeof(BitVector).AssemblyQualifiedName.GetHashCode();

		readonly int _count;
		readonly BitFlags32[] _flags;

		/// <summary>
		///   Creates a new instance.
		/// </summary>
		/// <param name="count">Number of bits contained in the vector</param>
		public BitVector(int count)
		{
			Contract.Requires(count >= 0, "count must be greater than or equal to zero");
			_count = count;
			_flags = (count == 0) ? new BitFlags32[0] : new BitFlags32[(count / BitFlags32.CFlagCount) + 1];
		}

		/// <summary>
		///   Gets and sets the bit at the index given.
		/// </summary>
		/// <param name="index">index of the bit to set or get; zero based.</param>
		/// <returns>whether the bit at the given index is turned on</returns>
		public bool this[int index]
		{
			get
			{
				if (index < 0 || index >= _count)
				{
					throw new ArgumentOutOfRangeException("index", Resources.Err_index_out_of_range);
				}

				var i = index / BitFlags32.CFlagCount;
				var bit = index % BitFlags32.CFlagCount;
				return _flags[i][bit];
			}
			set
			{
				if (index < 0 || index >= _count)
				{
					throw new ArgumentOutOfRangeException("index", Resources.Err_index_out_of_range);
				}

				var i = index / BitFlags32.CFlagCount;
				var bit = index % BitFlags32.CFlagCount;
				if (_flags[i][bit] != value)
				{
					if (value)
					{
						_flags[i] = _flags[i].On(bit);
					}
					else
					{
						_flags[i] = _flags[i].Off(bit);
					}
				}
			}
		}

		/// <summary>
		///   Number of flags in the vector.
		/// </summary>
		public int Count
		{
			get { return _count; }
		}

		/// <summary>
		///   Determines if the vector is empty.
		/// </summary>
		public bool IsEmpty
		{
			get { return _count == 0; }
		}

		/// <summary>
		///   Number of flags currently set to true.
		/// </summary>
		public int TrueFlagCount
		{
			get
			{
				var result = 0;
				if (_count > 0)
				{
// ReSharper disable LoopCanBeConvertedToQuery
					foreach (var flags in _flags)
					{
						result += flags.TrueFlagCount;
					}
// ReSharper restore LoopCanBeConvertedToQuery
				}
				return result;
			}
		}

		/// <summary>
		///   Determines if the vector is equal to another object.
		/// </summary>
		/// <param name="obj">the other object</param>
		/// <returns>true if equal; otherwise false</returns>
		public override bool Equals(object obj)
		{
			return obj is BitVector
				&& Equals((BitVector) obj);
		}

		/// <summary>
		///   Gets the hashcode for the vector.
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			const int prime = Constants.NotSoRandomPrime;

			var result = CHashCodeSeed * prime;
			result ^= _count * prime;
			if (_flags != null)
			{
				int index = 0;
				for (; index < this._flags.Length; index++)
				{
					var item = this._flags[index];
					result ^= item * prime;
				}
			}
			return result;
		}

		/// <summary>
		///   Converts the bit vector into a bit string.
		/// </summary>
		/// <returns>bits string</returns>
		public override string ToString()
		{
			if (_flags == null || _flags.Length == 0)
			{
				return BitFlags32.Empty.ToString();
			}

			var result = new StringBuilder(_flags.Length * BitFlags32.CFlagCount);
			foreach (var flags in _flags)
			{
				result.Append(flags.ToString());
			}
			return result.ToString();
		}

		/// <summary>
		///   Makes a copy.
		/// </summary>
		/// <returns></returns>
		public BitVector Copy()
		{
			var copy = new BitVector(_count);
			for (var i = 0; i < _flags.Length; i++)
			{
				copy._flags[i] = _flags[i];
			}
			return copy;
		}

		/// <summary>
		///   Gets the BitFlags32 at the index given.
		/// </summary>
		/// <param name="index"></param>
		/// <returns></returns>
		public BitFlags32 GetFlags(int index) { return _flags[index]; }

		/// <summary>
		///   Sets the BitFlags32 at the index given.
		/// </summary>
		/// <param name="index"></param>
		/// <param name="value"></param>
		/// <returns></returns>
		public BitVector SetFlags(int index, BitFlags32 value)
		{
			_flags[index] = value;
			return this;
		}

		/// <summary>
		///   Equality operator.
		/// </summary>
		/// <param name="lhs">left hand comparand</param>
		/// <param name="rhs">right hand comparand</param>
		/// <returns>true if the comparands are equal; otherwise false</returns>
		public static bool operator ==(BitVector lhs, BitVector rhs) { return lhs.Equals(rhs); }

		/// <summary>
		///   Inequality operator.
		/// </summary>
		/// <param name="lhs">left hand comparand</param>
		/// <param name="rhs">right hand comparand</param>
		/// <returns>true if the comparands are NOT equal; otherwise false</returns>
		public static bool operator !=(BitVector lhs, BitVector rhs) { return !lhs.Equals(rhs); }

		#region ICloneable Members

		/// <summary>
		///   Clones the current instance.
		/// </summary>
		/// <returns></returns>
		public object Clone() { return Copy(); }

		#endregion

		#region IEquatable<BitVector> Members

		/// <summary>
		///   Determines if the vector is equal to another.
		/// </summary>
		/// <param name="other">the other vector</param>
		/// <returns>true if equal; otherwise false.</returns>
		public bool Equals(BitVector other)
		{
			if (_count == other._count)
			{
				if (_flags == null || _flags.Length == 0)
				{
					return other.IsEmpty;
				}
// ReSharper disable LoopCanBeConvertedToQuery
				for (var i = 0; i < _flags.Length; i++)
				{
					if (_flags[i] != other._flags[i])
					{
						return false;
					}
				}
// ReSharper restore LoopCanBeConvertedToQuery
				return true;
			}
			return false;
		}

		#endregion
	}
}