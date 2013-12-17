﻿using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using FlitBit.Core.Parallel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;

namespace FlitBit.Core.Tests.Parallel
{
	[TestClass]
	public class ParallelReactorTests
	{
    [TestCleanup]
    public void Cleanup()
    {
      UncaughtExceptionTrap.CheckUncaughtException();
    }

		[TestMethod]
		public void Reactor_GetsNotifiedForAllItemsGiven()
		{
			var test = new
			{
				MaxWaitMilliseconds = 100,
				Items = 1000,
				// Ensure the foreground thread waits for shorter periods
				// in order to force parallelism.
				ForegroundThreadForceFactor = 0.3,
				MaxDegreeOfParallelism = 12
			};
			var foregroundRand = new Random(Environment.TickCount);
			var collector = new ConcurrentBag<Notification<int>>();
			var order = 0;

			var reactor = new Reactor<int>((r, i) =>
			{
				// Inside the background/thread-pool thread:
				// 1. Determine the number of milliseconds to wait.
				// 2. Record the item for printed output (convenience).
				// 3. Simulate other work, forcing parallelism, by making the thread wait.
				var wait = new Random(Environment.TickCount).Next(test.MaxWaitMilliseconds, test.MaxWaitMilliseconds);
        Thread.Sleep(wait);
				collector.Add(new Notification<int>
				{
					Item = i,
					ThreadID = Thread.CurrentThread.ManagedThreadId,
					Order = Interlocked.Increment(ref order),
          Wait = wait
				});
			}, new ReactorOptions(test.MaxDegreeOfParallelism));

			// Inside the foreground thread:
			// 1. Determine the number of milliseconds to wait (push faster than the reactor notifies)
			// 2. Push an item; we're using "i" so that the items have an order on the way in.
			//    This does not mean that they will have the same order in the printed results.
			//    It is instructive to eyeball the test's output and see that there are multiple
			//    background threads overlapping the results.
			// 3. Simulate other activity by waiting.
			for (var i = 0; i < test.Items; i++)
			{
				var wait = foregroundRand.Next(Convert.ToInt32(test.MaxWaitMilliseconds * test.ForegroundThreadForceFactor));
        Thread.Sleep(wait);
        reactor.Push(i);
			}

		  var assumeFailure = DateTime.Now.Add(TimeSpan.FromMinutes(1));
			// Spinwait for reactor to be empty...
			while (order < test.Items)
			{
			  if (DateTime.Now > assumeFailure)
			  {
			    Assert.Fail("Reactor failed to notify us of all of the items before the timeout.");
			  }
				Thread.Sleep(200);
			}
		  Assert.IsTrue(reactor.IsEmpty);

			Assert.AreEqual(test.Items, collector.Count);
			foreach (var item in collector.OrderBy(i => i.Order))
			{
				Console.WriteLine(String.Concat(item.Order, ": background thread ", item.ThreadID, " received item ", item.Item, " and waited ", item.Wait, " milliseconds."));
			}
		}

		class Notification<TItem>
		{
			public TItem Item { get; set; }
			public int Order { get; set; }
			public int ThreadID { get; set; }
      public int Wait { get; set; }
		}
	}
}