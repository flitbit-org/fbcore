using System;
using System.Collections.Generic;
using System.Threading;
using FlitBit.Core.Parallel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Parallel
{
	[TestClass]
	public class DemuxProducerTests
	{
		class Observation
		{
			public int Sequence { get; set; }
			public int Producer { get; set; }
			public int Observer { get; set; }
			public int Latency { get; set; }
		}

		class TestDemuxer : DemuxProducer<int, Observation>
		{
			static int __sequence = 0;

			protected override bool ProduceResult(int arg, out Observation value)
			{
				int wait = 0; // new Random().Next(50);
				Thread.Sleep(wait);
				value = new Observation
				{
					Sequence = Interlocked.Increment(ref __sequence),
					Producer = Thread.CurrentThread.ManagedThreadId,
					Latency = wait
				};
				return true;
			}
		}

		[TestMethod]
		public void DemuxProducer_CanDemultiplexOps()
		{
			var test = new
			{
				Threads = 12,
				Iterations = 1000000,
				Max = 100
			};
			int originators = 0;
			int observers = 0;
			int blanks = 0;
			int fails = 0;
			var demux = new TestDemuxer();
			var threads = new List<Thread>();
			Exception ex = null;
			for (var i = 0; i < test.Threads; i++)
			{
				var thread = new Thread(new ThreadStart(() =>
				{
					Random rand = new Random();
					try
					{
						for (int j = 0; j < test.Iterations; j++)
						{
							Observation value;
							int item = rand.Next(test.Max);
							var kind = demux.TryConsume(item, out value);
							if (value == null)
							{
								Interlocked.Increment(ref fails);
							}
							if (kind.HasFlag(DemuxResultKind.Originated))
							{
								Interlocked.Increment(ref originators);
							}
							else if (kind.HasFlag(DemuxResultKind.Observed))
							{
								Interlocked.Increment(ref observers);
							}
							else
							{
								Interlocked.Increment(ref blanks);
							}
						}
					}
					catch (Exception e)
					{
						ex = e;
					}
				}));
				thread.Start();
				threads.Add(thread);
			}

			foreach (var th in threads)
			{
				th.Join();
			}
			if (ex != null) throw ex;

			Console.Out.WriteLine(String.Concat("Originators: ", originators));
			Console.Out.WriteLine(String.Concat("Observers: ", observers));
			Console.Out.WriteLine(String.Concat("Blanks: ", blanks));
			Console.Out.WriteLine(String.Concat("Fails: ", fails));
			Assert.AreEqual(0, fails);
		}

		class ErrorProducer : DemuxProducer<int, object>
		{
			protected override bool ProduceResult(int arg, out object value)
			{
				Thread.Sleep(15);
				throw new InvalidOperationException("Yo, this be da bomb!");
			}
		}

		[TestMethod]
		public void DemuxProducer_ExceptionsThrownByProducerArePropagatedToDemuxedThreads()
		{
			var producer = new ErrorProducer();

			var sync = new Object();
			var threads = new Tuple<Thread, Object>[10];
			for (int i = 0; i < threads.Length; i++)
			{
				var tuple = new Tuple<Thread, Object>(new Thread(new ParameterizedThreadStart(n =>
				{
					int idx = (int)n;
					try
					{
						object obj;
						if (producer.TryConsume(0, out obj).HasFlag(DemuxResultKind.Observed))
						{
							lock (sync)
							{
								var tpl = threads[idx];
								threads[idx] = new Tuple<Thread, object>(tpl.Item1, obj);
							}
						}
					}
					catch (Exception e)
					{
						lock (sync)
						{
							var tpl = threads[idx];
							threads[idx] = new Tuple<Thread, object>(tpl.Item1, e);
						}
					}
				})), null);
				threads[i] = tuple;
				tuple.Item1.Start(i);
			}
			foreach (var tpl in threads)
			{
				tpl.Item1.Join();
			}
			lock (sync)
			{
				for (var i = 0; i < threads.Length; i++)
				{
					Assert.IsNotNull(threads[i].Item2);
				}
			}
		}
	}
}
