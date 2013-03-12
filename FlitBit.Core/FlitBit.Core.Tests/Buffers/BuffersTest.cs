using System;
using System.Collections.Generic;
using FlitBit.Core.Buffers;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Buffers
{
	[TestClass]
	public class BuffersTests
	{
		[TestMethod]
		public void AccessBufferReader_Default()
		{
			var reader = BufferReader.Create();
			Assert.IsNotNull(reader);
		}

		[TestMethod]
		public void AccessBufferWriter_Default()
		{
			var writer = BufferWriter.Create();
			Assert.IsNotNull(writer);
		}

		[TestMethod]
		public void BufferRoundtrip_Bool()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var test = new
				{
					Items = 200,
					SizeOfItem = sizeof(bool),
					Next = new Func<bool>(gen.GetBoolean),
					Recorded = new Queue<bool>()
				};

			var buffer = new byte[test.Items*test.SizeOfItem];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				var written = writer.Write(buffer, ref writeCursor, value);

				Assert.AreEqual(test.SizeOfItem, written, String.Concat("should have written ", test.SizeOfItem));
				Assert.AreEqual(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var bufferedValue = reader.ReadBoolean(buffer, ref readCursor);

				Assert.AreEqual(test.SizeOfItem, readCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
				Assert.AreEqual(value, bufferedValue, "value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod]
		public void BufferRoundtrip_Byte()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var test = new
				{
					Items = 4000,
					SizeOfItem = sizeof(byte),
					Next = new Func<byte>(gen.GetByte),
					Recorded = new Queue<byte>()
				};

			var buffer = new byte[test.Items*test.SizeOfItem];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				var written = writer.Write(buffer, ref writeCursor, value);

				Assert.AreEqual(test.SizeOfItem, written, String.Concat("should have written ", test.SizeOfItem));
				Assert.AreEqual(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var bufferedValue = reader.ReadByte(buffer, ref readCursor);

				Assert.AreEqual(test.SizeOfItem, readCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
				Assert.AreEqual(value, bufferedValue, "value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod]
		public void BufferRoundtrip_ByteArray()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var rand = new Random(Environment.TickCount);

			var test = new
				{
					Items = 4000,
					SizeOfItem = 4000,
					Next = new Func<byte[]>(() => gen.GetBytes(rand.Next(4000))),
					Recorded = new Queue<byte[]>()
				};

			var buffer = new byte[test.Items*(test.SizeOfItem*sizeof(Int32))];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				// Write a length prefix so we know how big the arrays are comming back out...
				var written = writer.Write(buffer, ref writeCursor, value.Length);
				written += writer.Write(buffer, ref writeCursor, value, 0, value.Length);
				Assert.AreEqual(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var byteCount = reader.ReadInt32(buffer, ref readCursor);
				Assert.AreEqual(value.Length, byteCount, "value read from buffer should be the same as the value we recorded");

				var bufferedValue = reader.ReadBytes(buffer, ref readCursor, byteCount);
				Assert.IsTrue(value.EqualsOrItemsEqual(bufferedValue),
											"value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod]
		public void BufferRoundtrip_ByteArrayPart()
		{
			var writer = BufferWriter.Create();
			Assert.IsNotNull(writer);
			var reader = BufferReader.Create();
			Assert.IsNotNull(reader);

			var rand = new Random(Environment.TickCount);
			var count = 2000;
			var cursor = 0;
			var buffer = new byte[200];
			var wvalue = new byte[count];
			rand.NextBytes(wvalue);
			var start = rand.Next(wvalue.Length - buffer.Length);
			writer.Write(buffer, ref cursor, wvalue, start, buffer.Length);
			Assert.AreEqual<int>(cursor, buffer.Length);

			cursor = 0;

			var rvalue = reader.ReadBytes(buffer, ref cursor, buffer.Length);
			Assert.AreEqual<int>(cursor, buffer.Length);
			for (var i = 0; i < buffer.Length; i++)
			{
				Assert.AreEqual<byte>(wvalue[i + start], rvalue[i]);
			}
		}

		[TestMethod]
		public void BufferRoundtrip_Char()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var test = new
				{
					Items = 4000,
					SizeOfItem = sizeof(char),
					Next = new Func<char>(() => gen.GetChar()),
					Recorded = new Queue<char>()
				};

			var buffer = new byte[test.Items*test.SizeOfItem];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				var written = writer.Write(buffer, ref writeCursor, value);

				Assert.AreEqual<int>(test.SizeOfItem, written, String.Concat("should have written ", test.SizeOfItem));
				Assert.AreEqual<int>(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var bufferedValue = reader.ReadChar(buffer, ref readCursor);

				Assert.AreEqual<int>(test.SizeOfItem, readCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
				Assert.AreEqual(value, bufferedValue, "value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod]
		public void BufferRoundtrip_Int16()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var test = new
				{
					Items = 4000,
					SizeOfItem = sizeof(Int16),
					Next = new Func<Int16>(() => gen.GetInt16()),
					Recorded = new Queue<Int16>()
				};

			var buffer = new byte[test.Items*test.SizeOfItem];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				var written = writer.Write(buffer, ref writeCursor, value);

				Assert.AreEqual<int>(test.SizeOfItem, written, String.Concat("should have written ", test.SizeOfItem));
				Assert.AreEqual<int>(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var bufferedValue = reader.ReadInt16(buffer, ref readCursor);

				Assert.AreEqual<int>(test.SizeOfItem, readCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
				Assert.AreEqual(value, bufferedValue, "value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod]
		public void BufferRoundtrip_UInt16()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var test = new
				{
					Items = 4000,
					SizeOfItem = sizeof(UInt16),
					Next = new Func<UInt16>(() => gen.GetUInt16()),
					Recorded = new Queue<UInt16>()
				};

			var buffer = new byte[test.Items*test.SizeOfItem];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				var written = writer.Write(buffer, ref writeCursor, value);

				Assert.AreEqual<int>(test.SizeOfItem, written, String.Concat("should have written ", test.SizeOfItem));
				Assert.AreEqual<int>(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var bufferedValue = reader.ReadUInt16(buffer, ref readCursor);

				Assert.AreEqual<int>(test.SizeOfItem, readCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
				Assert.AreEqual(value, bufferedValue, "value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod]
		public void BufferRoundtrip_Int32()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var test = new
				{
					Items = 4000,
					SizeOfItem = sizeof(Int32),
					Next = new Func<Int32>(() => gen.GetInt32()),
					Recorded = new Queue<Int32>()
				};

			var buffer = new byte[test.Items*test.SizeOfItem];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				var written = writer.Write(buffer, ref writeCursor, value);

				Assert.AreEqual<int>(test.SizeOfItem, written, String.Concat("should have written ", test.SizeOfItem));
				Assert.AreEqual<int>(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var bufferedValue = reader.ReadInt32(buffer, ref readCursor);

				Assert.AreEqual<int>(test.SizeOfItem, readCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
				Assert.AreEqual(value, bufferedValue, "value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod]
		public void BufferRoundtrip_UInt32()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var test = new
				{
					Items = 4000,
					SizeOfItem = sizeof(UInt32),
					Next = new Func<UInt32>(() => gen.GetUInt32()),
					Recorded = new Queue<UInt32>()
				};

			var buffer = new byte[test.Items*test.SizeOfItem];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				var written = writer.Write(buffer, ref writeCursor, value);

				Assert.AreEqual<int>(test.SizeOfItem, written, String.Concat("should have written ", test.SizeOfItem));
				Assert.AreEqual<int>(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var bufferedValue = reader.ReadUInt32(buffer, ref readCursor);

				Assert.AreEqual<int>(test.SizeOfItem, readCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
				Assert.AreEqual(value, bufferedValue, "value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod]
		public void BufferRoundtrip_Int64()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var test = new
				{
					Items = 4000,
					SizeOfItem = sizeof(Int64),
					Next = new Func<Int64>(() => gen.GetInt64()),
					Recorded = new Queue<Int64>()
				};

			var buffer = new byte[test.Items*test.SizeOfItem];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				var written = writer.Write(buffer, ref writeCursor, value);

				Assert.AreEqual<int>(test.SizeOfItem, written, String.Concat("should have written ", test.SizeOfItem));
				Assert.AreEqual<int>(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var bufferedValue = reader.ReadInt64(buffer, ref readCursor);

				Assert.AreEqual<int>(test.SizeOfItem, readCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
				Assert.AreEqual(value, bufferedValue, "value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod]
		public void BufferRoundtrip_UInt64()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var test = new
				{
					Items = 4000,
					SizeOfItem = sizeof(UInt64),
					Next = new Func<UInt64>(() => gen.GetUInt64()),
					Recorded = new Queue<UInt64>()
				};

			var buffer = new byte[test.Items*test.SizeOfItem];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				var written = writer.Write(buffer, ref writeCursor, value);

				Assert.AreEqual<int>(test.SizeOfItem, written, String.Concat("should have written ", test.SizeOfItem));
				Assert.AreEqual<int>(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var bufferedValue = reader.ReadUInt64(buffer, ref readCursor);

				Assert.AreEqual<int>(test.SizeOfItem, readCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
				Assert.AreEqual(value, bufferedValue, "value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod]
		public void BufferRoundtrip_Decimal()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var test = new
				{
					Items = 4000,
					SizeOfItem = sizeof(Decimal),
					Next = new Func<Decimal>(() => gen.GetDecimal()),
					Recorded = new Queue<Decimal>()
				};

			var buffer = new byte[test.Items*test.SizeOfItem];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				var written = writer.Write(buffer, ref writeCursor, value);

				Assert.AreEqual<int>(test.SizeOfItem, written, String.Concat("should have written ", test.SizeOfItem));
				Assert.AreEqual<int>(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var bufferedValue = reader.ReadDecimal(buffer, ref readCursor);

				Assert.AreEqual<int>(test.SizeOfItem, readCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
				Assert.AreEqual(value, bufferedValue, "value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod]
		public void BufferRoundtrip_Double()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var test = new
				{
					Items = 4000,
					SizeOfItem = sizeof(Double),
					Next = new Func<Double>(() => gen.GetDouble()),
					Recorded = new Queue<Double>()
				};

			var buffer = new byte[test.Items*test.SizeOfItem];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				var written = writer.Write(buffer, ref writeCursor, value);

				Assert.AreEqual<int>(test.SizeOfItem, written, String.Concat("should have written ", test.SizeOfItem));
				Assert.AreEqual<int>(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var bufferedValue = reader.ReadDouble(buffer, ref readCursor);

				Assert.AreEqual<int>(test.SizeOfItem, readCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
				Assert.AreEqual(value, bufferedValue, "value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod]
		public void BufferRoundtrip_Single()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var test = new
				{
					Items = 4000,
					SizeOfItem = sizeof(Single),
					Next = new Func<Single>(() => gen.GetSingle()),
					Recorded = new Queue<Single>()
				};

			var buffer = new byte[test.Items*test.SizeOfItem];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				var written = writer.Write(buffer, ref writeCursor, value);

				Assert.AreEqual<int>(test.SizeOfItem, written, String.Concat("should have written ", test.SizeOfItem));
				Assert.AreEqual<int>(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var bufferedValue = reader.ReadSingle(buffer, ref readCursor);

				Assert.AreEqual<int>(test.SizeOfItem, readCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
				Assert.AreEqual(value, bufferedValue, "value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod]
		public void BufferRoundtrip_Guid()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var test = new
				{
					Items = 4000,
					SizeOfItem = sizeof(byte)*16,
					Next = new Func<Guid>(() => gen.GetGuid()),
					Recorded = new Queue<Guid>()
				};

			var buffer = new byte[test.Items*test.SizeOfItem];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				var written = writer.Write(buffer, ref writeCursor, value);

				Assert.AreEqual<int>(test.SizeOfItem, written, String.Concat("should have written ", test.SizeOfItem));
				Assert.AreEqual<int>(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var bufferedValue = reader.ReadGuid(buffer, ref readCursor);

				Assert.AreEqual<int>(test.SizeOfItem, readCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
				Assert.AreEqual(value, bufferedValue, "value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod]
		public void BufferRoundtrip_LengthPrefixedString()
		{
			var writer = BufferWriter.Create();
			var reader = BufferReader.Create();
			var gen = new DataGenerator();
			var rand = new Random(Environment.TickCount);

			var test = new
				{
					Items = 4000,
					SizeOfItem = 4000,
					Next = new Func<string>(() => gen.GetString(rand.Next(4000))),
					Recorded = new Queue<string>()
				};

			// ~ 640M
			var buffer = new byte[test.Items*test.SizeOfItem*4];
			var writeCursor = 0;
			for (var i = 0; i < test.Items; i++)
			{
				var captureCursor = writeCursor;
				var value = test.Next();
				test.Recorded.Enqueue(value);

				var written = writer.Write(buffer, ref writeCursor, value, true);
				Assert.AreEqual<int>(written, writeCursor - captureCursor,
														"cursor should have incremented equal to the number of bytes written");
			}

			var readCursor = 0;
			while (test.Recorded.Count > 0)
			{
				var captureCursor = readCursor;
				var value = test.Recorded.Dequeue();
				var bufferedValue = reader.ReadStringWithByteCountPrefix(buffer, ref readCursor);

				Assert.AreEqual(value, bufferedValue, "value read from buffer should be the same as the value we recorded");
			}
		}

		[TestMethod, ExpectedException(typeof(ArgumentOutOfRangeException))]
		public void Buffer_BufferOverrunInt16()
		{
			var writer = BufferWriter.Create();
			var gen = new DataGenerator();
			var test = new
				{
					Items = 4,
					SizeOfItem = sizeof(Int16),
				};

			var buffer = new byte[test.Items*test.SizeOfItem];
			for (var i = 0; i < test.SizeOfItem; i++)
			{
				var writeCursor = (test.Items*test.SizeOfItem) - i;
				writer.Write(buffer, ref writeCursor, gen.GetInt16());
				Assert.Fail("Should have thrown an error because of the buffer overrun");
			}
		}
	}
}