using System;
using System.Linq;
using System.Threading;
using FlitBit.Core.Parallel;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace FlitBit.Core.Tests.Parallel
{
	[TestClass]
	public class ContextFlowParallelTests
	{
    [TestCleanup]
    public void Cleanup()
    {
      UncaughtExceptionTrap.CheckUncaughtException();
    }

		[TestMethod]
		public void Parallel_ErrorPropagatesToErrorHandler()
		{
			Exception caught = null;
			var completed = false;
      var assumeFailure = DateTime.Now.Add(TimeSpan.FromMinutes(1));
      

			using (var completion = ContextFlow.ParallelWithCompletion(
																											 () =>
																											 {
																												Thread.Sleep(TimeSpan.FromSeconds(0.1));
																												throw new InvalidOperationException("Kaboom!");
																											 }))
			{
				completion.Continue(
													 e =>
													 {
														caught = e;
														completed = true;
													 });

        // Spinwait for completion...
        while (!completed)
        {
          if (DateTime.Now > assumeFailure)
          {
            Assert.Fail("Failed to complete before the timeout.");
          }
          Thread.Sleep(200);
        }
      }

			Assert.IsNotNull(caught);
			Assert.AreEqual("Kaboom!", caught.Message);
			Assert.IsTrue(completed);
		}

		[TestMethod]
		public void Parallel_ErrorThrownByErrorHandlerCausesOnUncaughtException()
		{
			Exception caught = null;
			Exception uncaught = null;
			var completed = false;
      var assumeFailure = DateTime.Now.Add(TimeSpan.FromMinutes(1));

			ContextFlow.OnUncaughtException += (sender, e) => { uncaught = e.Error; };

			using (var completion = ContextFlow.ParallelWithCompletion(
																											 () =>
																											 {
																												Thread.Sleep(TimeSpan.FromSeconds(0.5));
																												throw new InvalidOperationException("Kaboom!");
																											 }))
			{
				completion.Continue(
													 e =>
													 {
														caught = e;
														completed = true;
														throw new InvalidOperationException("Whammy!");
													 });

				Assert.IsFalse(completion.IsCompleted);
				Assert.IsFalse(completion.IsFaulted);

        // Spinwait for completion...
        while (!completed || caught == null || uncaught == null)
        {
          if (DateTime.Now > assumeFailure)
          {
            Assert.Fail("Failed to complete before the timeout.");
          }
          Thread.Sleep(200);
        }

        Assert.IsTrue(completed);
        Assert.IsTrue(completion.IsCompleted);
				Assert.IsTrue(completion.IsFaulted, "faulted due to the exception 'Kaboom!'");

        Assert.IsNotNull(caught);
        Assert.IsNotNull(uncaught);
				Assert.AreSame(caught, completion.Exception);
				Assert.AreEqual("Kaboom!", caught.Message);
				Assert.AreEqual("Whammy!", uncaught.Message);
			}
		}

		[TestMethod]
		public void Parallel_ExecuteFunctionInParallelAndGetResult()
		{
			Exception caught = null;
			var completed = false;
			var handbackTotal = 0;
      var assumeFailure = DateTime.Now.Add(TimeSpan.FromMinutes(1));

			using (var completion = ContextFlow.ParallelWithCompletion(
																											 () =>
																											 {
																												Thread.Sleep(TimeSpan.FromSeconds(1));
																												return new[] {1, 2, 3, 4, 5, 6, 7}.Sum();
																											 }))
			{
				completion.Continue(
													 (e, total) =>
													 {
														caught = e;
														completed = true;
														handbackTotal = total;
													 });

        // Spinwait for completion...
        while (!completed)
        {
          if (DateTime.Now > assumeFailure)
          {
            Assert.Fail("Failed to complete before the timeout.");
          }
          Thread.Sleep(200);
        }

				var result = completion.AwaitValue();

				Assert.IsNull(caught);
				Assert.IsTrue(completed);
				Assert.AreEqual(28, handbackTotal);
				Assert.AreEqual(handbackTotal, result);
			}
		}

		[TestMethod]
		public void Parallel_ExecuteInParallel()
		{
			Exception caught = null;
			var completed = false;
      var assumeFailure = DateTime.Now.Add(TimeSpan.FromMinutes(1));

			using (var completion = ContextFlow.ParallelWithCompletion(
																											 () => Thread.Sleep(TimeSpan.FromSeconds(1))))
			{
				completion.Continue(
													 e =>
													 {
														caught = e;
														completed = true;
													 });

				Assert.IsFalse(completion.IsCompleted);
				Assert.IsFalse(completion.IsFaulted);

        // Spinwait for completion...
        while (!completed)
        {
          if (DateTime.Now > assumeFailure)
          {
            Assert.Fail("Failed to complete before the timeout.");
          }
          Thread.Sleep(200);
        }

				Assert.IsTrue(completion.IsCompleted);
				Assert.IsFalse(completion.IsFaulted);
			}

			Assert.IsNull(caught);
			Assert.IsTrue(completed);
		}

		[TestMethod]
		public void Parallel_ExecuteInParallelObservedByContinuationAndObservedByEvent()
		{
			Exception caught = null;
			var completed = false;
			var observerCalled = false;
      var assumeFailure = DateTime.Now.Add(TimeSpan.FromMinutes(1));

			using (var completion = ContextFlow.ParallelWithCompletion(
																											 () => Thread.Sleep(TimeSpan.FromSeconds(1))))
			{
				completion.Continue(
													 e =>
													 {
														caught = e;
														completed = true;
													 });

				Assert.IsFalse(completion.IsCompleted);
				Assert.IsFalse(completion.IsFaulted);

				completion.Continue(e => { observerCalled = true; });

        // Spinwait for completion...
        while (!completed)
        {
          if (DateTime.Now > assumeFailure)
          {
            Assert.Fail("Failed to complete before the timeout.");
          }
          Thread.Sleep(200);
        }

				Assert.IsTrue(completion.Wait(TimeSpan.FromSeconds(2)));

				Assert.IsTrue(completion.IsCompleted);
				Assert.IsFalse(completion.IsFaulted);

				Assert.IsNull(caught);
				Assert.IsTrue(completed);
				Assert.IsTrue(observerCalled);
			}
		}

		[TestMethod]
		public void Parallel_ExecutionCanBeAwaited()
		{
      var assumeFailure = DateTime.Now.Add(TimeSpan.FromMinutes(1));

			using (var completion = ContextFlow.ParallelWithCompletion(
																											 () => Thread.Sleep(TimeSpan.FromSeconds(1))))
			{
				Assert.IsFalse(completion.IsCompleted);
				Assert.IsFalse(completion.IsFaulted);

        // Spinwait for completion...
        while (!completion.IsCompleted)
        {
          if (DateTime.Now > assumeFailure)
          {
            Assert.Fail("Failed to complete before the timeout.");
          }
          Thread.Sleep(200);
        }

				Assert.IsFalse(completion.IsFaulted);
			}
		}
	}
}