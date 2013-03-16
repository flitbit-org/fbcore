#region COPYRIGHT© 2009-2012 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FlitBit.Core.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Collections
{
	[TestClass]
	public class BitVectorTests
	{
		[TestMethod]
		public void BitVector_AccuratelyTracksBitsSetWhenSerializedRoundTrip()
		{
			const int bitCount = 78;
			const int bitTransitions = 10000;

			var rand = new Random(Environment.TickCount);
			var bits = new BitVector(bitCount);
			Assert.IsFalse(bits.IsEmpty);
			Assert.AreEqual(bitCount, bits.Count);
			Assert.AreEqual(0, bits.TrueFlagCount);
			for (var i = 0; i < bits.Count; i++)
			{
				Assert.IsFalse(bits[i]);
			}

			BitVector other;
			for (var i = 0; i < bitTransitions; i++)
			{
				var transition = rand.Next(bitCount - 1);
				using (var serialized = SerializeToStream(bits))
				{
					bits[transition] = !bits[transition];
					other = DeserializeFromStream<BitVector>(serialized);
				}

				for (var j = 0; j < bits.Count; j++)
				{
					if (j == transition)
					{
						Assert.AreNotEqual(bits[j], other[j]);
					}
					else
					{
						Assert.AreEqual(bits[j], other[j]);
					}
				}
			}
		}

		[TestMethod]
		public void BitVector_CorrectlyRecordsBitStates()
		{
			var input = new
			{
				Cycles = 50,
				Fetches = 1000000
			};
			var rand = new Random(Environment.TickCount);

			for (var cycle = 0; cycle < input.Cycles; cycle++)
			{
				var bitCount = rand.Next(1, 128);

				var bits = new BitVector(bitCount);

				var values = new List<bool>();

				Assert.AreEqual(bitCount, bits.Count);
				for (var i = 0; i < bits.Count; i++)
				{
					Assert.IsFalse(bits[i]);
					values.Add((rand.Next() % 3) == 0);
					bits[i] = values[i];
				}

				try
				{
					Assert.IsTrue(bits[bitCount + 1]);
					Assert.Fail("Should have thrown an out of range exception");
				}
				catch
				{}

				for (var i = 0; i < input.Fetches; i++)
				{
					var index = rand.Next(bits.Count);
					Assert.AreEqual(values[index], bits[index]);
				}
			}
		}

		[TestMethod]
		public void BitVector_EmptyIsEmpty()
		{
			var bits = default(BitVector);
			Assert.IsTrue(bits.IsEmpty);
			Assert.AreEqual(0, bits.Count);
			Assert.AreEqual(0, bits.TrueFlagCount);
			try
			{
				Assert.IsFalse(bits[1]);
				Assert.Fail();
			}
			catch (ArgumentOutOfRangeException)
			{}

			var another = default(BitVector);
			Assert.IsTrue(bits == another);
			Assert.AreEqual(bits, another);
			Assert.AreEqual(bits.GetHashCode(), another.GetHashCode());

			var zero = new BitVector(0);
			Assert.AreEqual(bits, zero);
			Assert.IsTrue(zero.IsEmpty);
			Assert.AreEqual(0, zero.Count);
			Assert.AreEqual(0, zero.TrueFlagCount);
			Assert.AreEqual(bits.GetHashCode(), zero.GetHashCode());
			try
			{
				Assert.IsFalse(zero[1]);
				Assert.Fail();
			}
			catch (ArgumentOutOfRangeException)
			{}
		}

		[TestMethod]
		public void BitVector_TwoInstancesWithEqualBitValuesAreEqual()
		{
			var bits = new BitVector(8);
			Assert.IsFalse(bits.IsEmpty);
			Assert.AreEqual(8, bits.Count);
			Assert.AreEqual(0, bits.TrueFlagCount);
			for (var i = 0; i < bits.Count; i++)
			{
				Assert.IsFalse(bits[i]);
			}

			bits[0] = true;
			bits[2] = true;
			bits[4] = true;
			bits[bits.Count - 1] = true;

			Assert.AreEqual(4, bits.TrueFlagCount);
			Assert.IsTrue(bits[0]);
			Assert.IsFalse(bits[1]);
			Assert.IsTrue(bits[2]);
			Assert.IsFalse(bits[3]);
			Assert.IsTrue(bits[4]);
			Assert.IsFalse(bits[6]);
			Assert.IsFalse(bits[6]);
			Assert.IsTrue(bits[7]);

			var zero = new BitVector(0);
			Assert.AreNotEqual(bits, zero);

			var other = new BitVector(8);
			other[0] = true;
			other[2] = true;
			other[4] = true;
			other[other.Count - 1] = true;

			Assert.AreEqual(bits, other);
			Assert.AreEqual(bits.GetHashCode(), other.GetHashCode());
		}

		static T DeserializeFromStream<T>(MemoryStream stream)
		{
			IFormatter formatter = new BinaryFormatter();
			stream.Seek(0, SeekOrigin.Begin);
			var o = (T) formatter.Deserialize(stream);
			return o;
		}

		static MemoryStream SerializeToStream<T>(T o)
		{
			var stream = new MemoryStream();
			IFormatter formatter = new BinaryFormatter();
			formatter.Serialize(stream, o);
			return stream;
		}
	}
}