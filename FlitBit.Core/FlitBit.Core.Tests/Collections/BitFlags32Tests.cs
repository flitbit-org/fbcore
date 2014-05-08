#region COPYRIGHT© 2009-2014 Phillip Clark. All rights reserved.

// For licensing information see License.txt (MIT style licensing).

#endregion

using System;
using System.Collections.Generic;
using FlitBit.Core.Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Collections
{
	[TestClass]
	public class BitFlags32Tests
	{
		[TestMethod]
		public void BitFlags32_CanSetIndividualBitsOnOff()
		{
			var bits = BitFlags32.Empty;

			// Initially appears completely empty...
			Assert.IsTrue(bits.IsEmpty);
			for (var i = 0; i < 32; i++)
			{
				Assert.IsFalse(bits[i]);
			}
			Assert.AreEqual(new String('0', 32), bits.ToString());
			Assert.AreEqual(0, (int) bits);
			Assert.AreEqual(0, bits.TrueFlagCount);

			var random = new Random();
			for (var i = 0; i < 1000; i++)
			{
				var bit = random.Next(31);
				bits = bits.On(bit);
				Assert.IsFalse(bits.IsEmpty);
				for (var b = 0; b < 32; b++)
				{
					if (b == bit)
					{
						Assert.IsTrue(bits[b]);
					}
					else
					{
						Assert.IsFalse(bits[b]);
					}
				}
				bits = bits.Off(bit);
				Assert.IsTrue(bits.IsEmpty);
			}
		}

		[TestMethod]
		public void BitFlags32_EmptyInstanceAppearsEmpty()
		{
			var empty = BitFlags32.Empty;
			Assert.IsTrue(empty.IsEmpty);
			for (var i = 0; i < 32; i++)
			{
				Assert.IsFalse(empty[i]);
			}
			Assert.AreEqual("00000000000000000000000000000000", empty.ToString());
			Assert.AreEqual(0, (int) empty);
			Assert.AreEqual(0, empty.TrueFlagCount);

			var anotherEmpty = (BitFlags32) 0;
			Assert.AreEqual(empty, anotherEmpty);
			Assert.IsTrue(EqualityComparer<BitFlags32>.Default.Equals(empty, anotherEmpty));

			var noLongerEmpty = empty.On(0);
			Assert.IsFalse(noLongerEmpty.IsEmpty);
			Assert.IsTrue(noLongerEmpty[0]);
		}
	}
}